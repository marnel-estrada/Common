using System;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace CommonEcs {
    [UpdateAfter(typeof(TransformVerticesSystem))]
    [UpdateAfter(typeof(SpriteManagerJobsFinisher))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SpriteManagerRendererSystem : ComponentSystem {
        private EntityQuery query;
        private SharedComponentQuery<SpriteManager> spriteManagerQuery;
        
        private readonly List<SpriteManager> sortedList = new List<SpriteManager>(1);

        protected override void OnCreateManager() {
            this.query = GetEntityQuery(typeof(SpriteManager), ComponentType.Exclude<Sprite>(),
                ComponentType.Exclude<MeshRendererVessel>());
            this.spriteManagerQuery = new SharedComponentQuery<SpriteManager>(this, this.EntityManager);
        }

        protected override void OnUpdate() {
            this.spriteManagerQuery.Update();
            NativeArray<ArchetypeChunk> chunks = this.query.CreateArchetypeChunkArray(Allocator.TempJob);
            AddAndSort(chunks);
            chunks.Dispose();
            
            // Render in order
            for (int i = 0; i < this.sortedList.Count; ++i) {
                SpriteManager spriteManager = this.sortedList[i];
                spriteManager.UpdateMesh();

                Graphics.DrawMesh(spriteManager.Mesh, Matrix4x4.identity, spriteManager.Material, spriteManager.Layer);
            }
            
            this.sortedList.Clear();
        }

        private static Comparison<SpriteManager> SORT_COMPARISON;

        private void AddAndSort(NativeArray<ArchetypeChunk> chunks) {
            for (int i = 0; i < chunks.Length; ++i) {
                ArchetypeChunk chunk = chunks[i];
                SpriteManager spriteManager = this.spriteManagerQuery.GetSharedComponent(ref chunk);
                
                // Note here that we only render if it was specified that it was not going to use a MeshRenderer
                if (!spriteManager.UseMeshRenderer) {
                    this.sortedList.Add(spriteManager);
                }
            }

            if (SORT_COMPARISON == null) {
                SORT_COMPARISON = Compare;
            }
            
            this.sortedList.Sort(SORT_COMPARISON);
        }

        private static int Compare(SpriteManager a, SpriteManager b) {
            if (a.SortingLayer < b.SortingLayer) {
                return -1;
            }
            
            if (a.SortingLayer > b.SortingLayer) {
                return 1;
            }

            return 0;
        }

    }
}
