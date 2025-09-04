using UnityEngine;

public class Player : MonoBehaviour {

    public void MoveForward(){

        transform.position += new Vector3(0.25f, 0, 0);
    }

    public void MoveBackward(){

        transform.position -= new Vector3(0.25f, 0, 0);
    }
}
