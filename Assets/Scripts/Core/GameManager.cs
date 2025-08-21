using UnityEngine;

namespace CuriousCity.Core
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance => _instance;
        
        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        
        void Start()
        {
            Debug.Log("[GameManager] Ready");
        }
    }
}