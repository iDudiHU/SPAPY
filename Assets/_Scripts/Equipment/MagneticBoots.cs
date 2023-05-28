using System;
using TigrisDigitalCreative._Input;
using UnityEngine;

namespace TigrisDigitalCreative._Scripts {
    public class MagneticBoots : MonoBehaviour
    {
        public bool IsActive { get; private set; }
        public InputManager inputManager;

        private void Awake()
        {
            inputManager = inputManager.GetComponent<InputManager>();
        }

        private void Activate()
        {
            IsActive = true;
        }
        private void Deactivate()
        {
            IsActive = false;
        }
        
        void Update()
        {
            if (inputManager.MagneticBootsIsOn)
            {
                Activate();
            }
            else
            {
                Deactivate();
            }
        }
    
        public bool IsMagnetic(GameObject gameObject)
        {
            return gameObject.GetComponent<IMagneticSurface>() != null;
        }
    }
}