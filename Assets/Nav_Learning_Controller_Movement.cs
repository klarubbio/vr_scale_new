using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nav_Learning_Controller_Movement : MonoBehaviour
{
    public bool xbox;
    public bool ps4;
    public float movementSpeed,rotationSpeed;
    private float xRot, zRot, horizontalTranslationInput,verticalTranslationInput, horizontalRotationInput, verticalRotationInput;
    // Start is called before the first frame update
    void Start()
    {
        //initialize x and z rotations (should be 0 by default based on the object)
        xRot = this.transform.eulerAngles.x;
        zRot = this.transform.eulerAngles.z;
    }

    // Update is called once per frame
    void Update()
    {
      if(xbox){
        horizontalTranslationInput = Input.GetAxis("xboxleft_horizontal") * movementSpeed * Time.deltaTime; //-Left +Right
        verticalTranslationInput = Input.GetAxis("xboxleft_vertical") * movementSpeed * Time.deltaTime;//-Backwards +Forward
        horizontalRotationInput = Input.GetAxis("xboxright_horizontal") * rotationSpeed * Time.deltaTime;//-Left +Right
        //Adding in functionality to vertical rotate camera due to size of buildings.
        verticalRotationInput = Input.GetAxis("xboxright_vertical") * rotationSpeed * Time.deltaTime;
      }else if(ps4){
        horizontalTranslationInput = Input.GetAxis("ps4left_horizontal") * movementSpeed * Time.deltaTime; //-Left +Right
        verticalTranslationInput = Input.GetAxis("ps4left_vertical") * movementSpeed * Time.deltaTime;//-Backwards +Forward
        horizontalRotationInput = Input.GetAxis("ps4right_horizontal") * rotationSpeed * Time.deltaTime;//-Left +Right
        //Adding in functionality to vertical rotate camera due to size of buildings.
        verticalRotationInput = Input.GetAxis("ps4right_vertical") * rotationSpeed * Time.deltaTime;

      }

        //rotate using the parent of the camera b/c we cannot directly manipulate the HMD rotation
        Camera.main.transform.parent.transform.Rotate(verticalRotationInput, horizontalRotationInput, 0);
        //handle translational movement (we use the camera's parent forward b/c of above [the player should be facing forward regardless])
        this.transform.position += new Vector3((Camera.main.transform.parent.transform.forward.x * verticalTranslationInput) + (Camera.main.transform.parent.transform.right.x * horizontalTranslationInput), 0, (Camera.main.transform.parent.transform.forward.z * verticalTranslationInput) + (Camera.main.transform.parent.transform.right.z * horizontalTranslationInput));
        //update the position of the entire player controller based on the parent of the camera within it
        this.transform.localEulerAngles = new Vector3(Camera.main.transform.parent.transform.localEulerAngles.x, Camera.main.transform.parent.transform.localEulerAngles.y, zRot);
    }
}

/*Mouse X
 * Gravity 0
 * Dead 0
 * Sensitivity 0.1
 * Mouse Movement
 * X Axis
 * Get Motion from all Joysticks
 *
 * Mouse Y
 * Gravity 0
 * Dead 0
 * Sensitivity 0.1
 * Mouse movement
 * Y axis
 * Get Motion from all Joysticks
 */
