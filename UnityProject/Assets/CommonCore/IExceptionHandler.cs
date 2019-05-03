using System;

namespace Common {
    public interface IExceptionHandler {
        void Handle(Exception e);
    }
}