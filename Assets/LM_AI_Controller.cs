using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(ThirdPersonCharacter))]
    public class LM_AI_Controller : MonoBehaviour
    {
        private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
        private Transform m_Cam;                  // A reference to the main camera in the scenes transform
        private Vector3 m_Move;
        

        public float playerSpeedMultiplier = 1.0f;
        public Transform goal;
        public static NavMeshAgent agent;

        private Experiment manager;

        private void Awake()
        {
            manager = GameObject.FindWithTag("Experiment").GetComponent<Experiment>();
        }


        private void Start()
        {
            

            // get the transform of the main camera
            if (Camera.main != null)
            {
                m_Cam = Camera.main.transform;
                //m_Cam.transform.position = new Vector3(this.transform.parent.transform.position.x, 50.0f, this.transform.parent.transform.position.z);
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
                // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            }

            agent = GetComponent<NavMeshAgent>();
            

            // get the third person character ( this should never be null due to require component )
            m_Character = GetComponent<ThirdPersonCharacter>();
        }


        private void Update()
        {

        }


        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {

            // read inputs -- MJS edited with ScaledNavigation button prefix to distinguish controls


        }
    }
}
