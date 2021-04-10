using Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GoapBrain {
    /// <summary>
    /// Handles interpreting GoapDomainData and turn it into domain actions and resolvers
    /// </summary>
    class GoapDomainInterpreter {
        private readonly GoapDomain domain;

        private ConditionNamesDatabase namesDb;

        /// <summary>
        /// Constructor with specified domain
        /// </summary>
        /// <param name="domain"></param>
        public GoapDomainInterpreter(GoapDomain domain) {
            this.domain = domain;
            this.namesDb = ConditionNamesDatabase.Instance;
        }

        /// <summary>
        /// Configures the specified data into the domain
        /// </summary>
        /// <param name="data"></param>
        public void Configure(GoapDomainData data) {
            // Copy parent variables
            this.domain.SetVariables(data.Variables);

            // Add all condition names to database
            for(int i = 0; i < data.ConditionNamesCount; ++i) {
                this.namesDb.GetOrAdd(data.GetConditionNameAt(i).Name);
            }

            // Actions
            for(int i = 0; i < data.ActionCount; ++i) {
                AddAction(data.GetActionAt(i), null);
            }

            // Condition resolvers
            for(int i = 0; i < data.ConditionResolvers.Count; ++i) {
                AddConditionResolver(data.ConditionResolvers[i]);
            }

            // Extensions
            for(int i = 0; i < data.Extensions.Count; ++i) {
                AddExtension(data.Extensions[i], i);
            }

            this.domain.Configure();
        }

        private void AddAction(GoapActionData actionData, List<Condition> extraPreconditions) {
#if UNITY_EDITOR
            // Try/catch only on editor
            try {
#endif
                if(!actionData.Enabled) {
                    // Not enabled, do not add
                    return;
                }

                GoapAction action = new GoapAction(actionData.Name);
                action.Cost = actionData.Cost;
                action.Cancellable = actionData.Cancellable;

                // Action Preconditions 
                AddPreconditions(action, actionData.Preconditions);

                // Extra Preconditions (like GOAP extensions)
                if (extraPreconditions != null) {
                    AddPreconditions(action, extraPreconditions); // Prefix is not added here because the preconditions are from a parent domain data
                }

                // Effects
                for (int i = 0; i < actionData.Effects.Count; ++i) {
                    // Note here that we add as copy
                    action.AddEffect(actionData.Effects[i].Name, actionData.Effects[i].Value);
                }

                // Actions
                for (int i = 0; i < actionData.AtomActions.Count; ++i) {
                    AddAtomicAction(action, actionData.AtomActions[i]);
                }

                this.domain.AddAction(action);
#if UNITY_EDITOR
            } catch(Exception e) {
                Debug.Log(actionData.Name);
                throw;
            }
#endif
        }

        private static void AddPreconditions(GoapAction action, List<Condition> preconditions) {
            for (int i = 0; i < preconditions.Count; ++i) {
                // Note here that we add as copy
                action.AddPrecondition(preconditions[i].Name, preconditions[i].Value);
            }
        }

        private void AddAtomicAction(GoapAction action, ClassData data) {
            // Instantiate the class
            Option<Type> type = TypeIdentifier.GetType(data.ClassName);
            Assertion.IsTrue(type.IsSome);
            type.Match(new AddAtomicActionMatcher(this, action, data));
        }

        private readonly struct AddAtomicActionMatcher : IOptionMatcher<Type> {
            private readonly GoapDomainInterpreter interpreter;
            private readonly GoapAction action;
            private readonly ClassData data;

            public AddAtomicActionMatcher(GoapDomainInterpreter interpreter, GoapAction action, ClassData data) {
                this.interpreter = interpreter;
                this.action = action;
                this.data = data;
            }

            public void OnSome(Type type) {
                ConstructorInfo constructor = TypeUtils.ResolveEmptyConstructor(type);
                GoapAtomAction atomAction = (GoapAtomAction)constructor.Invoke(TypeUtils.EMPTY_PARAMETERS);

                // Inject variables
                NamedValueUtils.InjectNamedProperties(this.interpreter.domain.Variables, data.Variables, type, atomAction);

                this.action.AddAtomAction(atomAction);
            }

            public void OnNone() {
            }
        }

        private void AddConditionResolver(ConditionResolverData data) {
            // Instantiate the precondition class
            Option<Type> type = TypeIdentifier.GetType(data.ResolverClass.ClassName);
            Assertion.IsTrue(type.IsSome);
            type.Match(new AddConditionResolverMatcher(this, data));
        }

        private readonly struct AddConditionResolverMatcher : IOptionMatcher<Type> {
            private readonly GoapDomainInterpreter interpreter;
            private readonly ConditionResolverData data;

            public AddConditionResolverMatcher(GoapDomainInterpreter interpreter, ConditionResolverData data) {
                this.interpreter = interpreter;
                this.data = data;
            }

            public void OnSome(Type type) {
                ConstructorInfo constructor = TypeUtils.ResolveEmptyConstructor(type);
                ConditionResolver resolver = (ConditionResolver)constructor.Invoke(TypeUtils.EMPTY_PARAMETERS);

                // Inject variables
                NamedValueUtils.InjectNamedProperties(this.interpreter.domain.Variables, data.ResolverClass.Variables, type, resolver);

                this.interpreter.domain.AddPreconditionResolver(this.data.ConditionName, resolver);
            }

            public void OnNone() {
            }
        }

        private void AddExtension(GoapExtensionData extension, int extensionIndex) {
            // Add the variables
            this.domain.AddVariables(extension.DomainData.Variables);

            // Condition resolvers
            for (int i = 0; i < extension.DomainData.ConditionResolvers.Count; ++i) {
                ConditionResolverData extensionData = extension.DomainData.ConditionResolvers[i];

                ConditionResolverData domainResolverData = this.domain.Data.GetConditionResolver(extensionData.ConditionName);
                if(domainResolverData != null) {
                    // A resolver for the same condition already exists
                    // We check that is has the same class
                    Assertion.IsTrue(extensionData.ResolverClass.ClassName.EqualsFast(domainResolverData.ResolverClass.ClassName), 
                        $"Precondition \"{extensionData.ConditionName}\" already exists in parent but has different resolver ({domainResolverData.ResolverClass.ClassName} - {extensionData.ResolverClass.ClassName}).");
                    continue; // No need to add
                }

                // Check previous extensions if the condition resolver has already been added
                // We also check that they have the same resolver
                if(extensionIndex > 0) {
                    ConditionResolverData resolverDataFromPreviousExtensions = FindConditionDataFromPreviousExtensions(extensionData, extensionIndex);
                    if(resolverDataFromPreviousExtensions != null) {
                        // A resolver for the same condition already exists
                        // We check that is has the same class
                        Assertion.IsTrue(extensionData.ResolverClass.ClassName.EqualsFast(resolverDataFromPreviousExtensions.ResolverClass.ClassName),
                            $"Precondition \"{extensionData.ConditionName}\" already exists in previous extensions but has different resolver ({resolverDataFromPreviousExtensions.ResolverClass.ClassName} - {extensionData.ResolverClass.ClassName}).");
                        continue; // No need to add
                    }
                }

                AddConditionResolver(extensionData);
            }

            // Actions
            for (int i = 0; i < extension.DomainData.ActionCount; ++i) {
                AddAction(extension.DomainData.GetActionAt(i), extension.Preconditions); // Note here that the extension's preconditions are added to the action
            }
        }

        private ConditionResolverData FindConditionDataFromPreviousExtensions(ConditionResolverData extensionData, int currentExtensionIndex) {
            if(currentExtensionIndex == 0) {
                // No nee
                return null;
            }

            for (int i = 0; i < currentExtensionIndex; ++i) {
                GoapExtensionData extension = this.domain.Data.Extensions[i];
                ConditionResolverData domainResolverData = extension.DomainData.GetConditionResolver(extensionData.ConditionName);
                if(domainResolverData != null) {
                    return domainResolverData;
                }
            }

            return null;
        }

    }
}
