using Photon.Pun;
using UnityEngine;

namespace CardsVR.Networking
{
    public class SingletonPUN<T> : MonoBehaviourPunCallbacks where T : Component
    {
        /*
         *      The Singleton design pattern ensures that only one gameObject
         *      can implement this component.  This pattern is generally used
         *      for game managers, including the Connection Manager.
         *      
         *      To implement this singleton, simply inherit this class.  
         *      Pass the child class type as parameter T.  The manager 
         *      class may then implement any desired UnityEngine methods 
         *      including PUN interfaces.
         */

        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                        _instance = obj.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        /*
         *      When this GameObject is initialized, it will first check if 
         *      another Singleton of this type has been initialized.  If so, 
         *      this duplicate Singleton is not initialized and instead the 
         *      corresponding gameObject is immediately destroyed iaw the
         *      Singleton design pattern.
         */
        public virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            } 
            else
            {
                Destroy(gameObject); // only allow one instance of type T to exist!
            }
        }
    }
}
