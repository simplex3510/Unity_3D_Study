using UnityEngine;

namespace Manager
{
    public class Singleton<T>
    {
        private static Singleton<T> instance;

        public static Singleton<T> Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Singleton<T>();
                }

                return instance;
            }
        }
    }
}
