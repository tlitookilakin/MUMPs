using System;

namespace MUMPs.Misc
{
    public class RLazy<T>
    {
        public T Value
        {
            get
            {
                if (inited)
                    return stored;

                stored = Factory();
                inited = true;
                return stored;
            }
        }
        public bool HasValue
        {
            get
            {
                return inited;
            }
        }
        private bool inited = false;
        private T stored;
        private Func<T> Factory;
        public RLazy(Func<T> factory)
        {
            Factory = factory;
        }
        public void Reset()
        {
            stored = Factory();
        }
    }
}
