using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class BallsFlightAdvanced : MonoBehaviour {

    // Balls from scene
    public GameObject ball1;
    public GameObject ball2;
    public GameObject ball3;
    public GameObject ball4;

    // Public game objects
    public Slider SpeedSlider;

    // Class with coordinates_ball1
    class Coordinates {
        public List<double> x;
        public List<double> y;
        public List<double> z;

        public Coordinates() {
            x = new List<double>();
            y = new List<double>();
            z = new List<double>();
        }

        //public List<double> X { get { return x; } set { x = value; } }
        //public List<double> Y { get { return y; } set { y = value; } }
        //public List<double> Z { get { return z; } set { z = value; } }
    }

    // Variables
    const string FILENAME1 = @"C:\Andrey\Dev\Repositories\Unity\TestTask\Assets\Resources\ball_path.json";
    const string FILENAME2 = @"C:\Andrey\Dev\Repositories\Unity\TestTask\Assets\Resources\ball_path2.json";
    const string FILENAME3 = @"C:\Andrey\Dev\Repositories\Unity\TestTask\Assets\Resources\ball_path3.json";
    const string FILENAME4 = @"C:\Andrey\Dev\Repositories\Unity\TestTask\Assets\Resources\ball_path4.json";
    Vector3 CAMERA_LOCAL_POSITION = new Vector3(-3, 7, -8);
    float speed;             // Current flight speed
    float curspeed;          // Counter for flight speed
    float firstClickTime;    // Time of first click
    int curposition;         // Current ball position
    int direction;           // Direction of ball moving
    bool isMoving;           // Is object moving

    // To control balls
    GameObject[] balls;
    int cur_ball_index;

    Coordinates coordinates_ball1; // List of coordinates_ball1 from ball_path.json
    Coordinates coordinates_ball2; // List of coordinates_ball1 from ball_path2.json
    Coordinates coordinates_ball3; // List of coordinates_ball1 from ball_path3.json
    Coordinates coordinates_ball4; // List of coordinates_ball1 from ball_path4.json

    List<Vector3> positions_ball1; // Ball positions_ball1
    List<Vector3> positions_ball2; // Ball positions_ball2
    List<Vector3> positions_ball3; // Ball positions_ball3
    List<Vector3> positions_ball4; // Ball positions_ball4
    List<Vector3>[] positions;     // All balls positions

    void Start () {

        // Check condition if objects is null
        if (ball1 == null || ball2 == null || ball3 == null
            || ball4 == null || SpeedSlider == null) {
            Debug.Log("One of the global method variables is null!");
            return;
        }

        coordinates_ball1 = new Coordinates();
        coordinates_ball2 = new Coordinates();
        coordinates_ball3 = new Coordinates();
        coordinates_ball4 = new Coordinates();

        // Init array of game objects Balls
        balls = new GameObject[4];
        positions = new List<Vector3>[4];
        balls[0] = ball1;
        balls[1] = ball2;
        balls[2] = ball3;
        balls[3] = ball4;
        cur_ball_index = 0;

        curspeed = 0;
        speed = SpeedSlider.value;
        curposition = 0;
        direction = 1;
        isMoving = false;
        firstClickTime = 0;

        // Parse all json files with balls' paths and rewrite it into the array of Vector3
        ParseJsonCoordinates(ReadFileToString(FILENAME1), out coordinates_ball1);
        CoordinatesToPositions(coordinates_ball1, out positions_ball1);
        ParseJsonCoordinates(ReadFileToString(FILENAME2), out coordinates_ball2);
        CoordinatesToPositions(coordinates_ball2, out positions_ball2);
        ParseJsonCoordinates(ReadFileToString(FILENAME3), out coordinates_ball3);
        CoordinatesToPositions(coordinates_ball3, out positions_ball3);
        ParseJsonCoordinates(ReadFileToString(FILENAME4), out coordinates_ball4);
        CoordinatesToPositions(coordinates_ball4, out positions_ball4);

        // Push all positions to array
        positions[0] = positions_ball1;
        positions[1] = positions_ball2;
        positions[2] = positions_ball3;
        positions[3] = positions_ball4;

        SpeedSlider.onValueChanged.AddListener(delegate { ChangeSpeed(); });

        // Reset all balls
        ResetBall(ball1, positions_ball1);
        ResetBall(ball2, positions_ball2);
        ResetBall(ball3, positions_ball3);
        ResetBall(ball4, positions_ball4);

        CheckCameraParent();
    }


    void Update () {

        // Check if balls positions is not empty
        if (positions_ball1.Count == 0 || positions_ball2.Count == 0 ||
            positions_ball3.Count == 0 || positions_ball4.Count == 0) {
            Debug.Log("One of the balls' positions array is empty!");
            return;
        }

        OnMouseDownOnBall();
        CheckRetargetBall();
        CheckInputOnCameraRotation();

        if (isMoving) {
            SpeedSlider.gameObject.SetActive(true);
            MoveBall();
        }
        else {
            SpeedSlider.gameObject.SetActive(false);
        }
	}


    // Read json file to Array of Lists
    bool ParseJsonCoordinates(string json_data, out Coordinates coordinates_ball1) {
        
        // Check if read string is empty
        if (json_data.Length == 0) {
            Debug.Log("BallFlight.ParseJsonCoordinates(): Json data is empty!");
            coordinates_ball1 = null;
            return false;
        }

        coordinates_ball1 = JsonUtility.FromJson<Coordinates>(json_data);
        return true;
    }


    // Read file to string
    string ReadFileToString(string filename) {
        string json_data = string.Empty;

        // Reading file
        try {
            using (StreamReader sr = new StreamReader(filename)) {
                json_data = sr.ReadToEnd();
            }
        }
        catch (Exception e) {
            Debug.Log("BallFlight.ReadFileToString(): Something went wrong in READING FILE");
            Debug.Log(e.Message);
        }

        return json_data;
    }


    /* Move ball with direction
     * direction == 1 by default
     * 
     * direction == 1 ball moves forward
     * direction == -1 ball moves backward
     */
    void MoveBall() {

        curspeed += speed;

        for (int i = 0; i < (int)curspeed; ++i) {

            curposition += direction;
            balls[cur_ball_index].transform.position = positions[cur_ball_index][curposition];
            curspeed = 0;

            // Check if ball reach the end point
            if (curposition >= positions[cur_ball_index].Count - 1 || curposition <= 0) {
                isMoving = false;
                direction *= -1;
                break;
            }
        }
    }


    // If mouse clicked on object
    void OnMouseDownOnBall() {

        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if (hit.transform.gameObject != balls[cur_ball_index])
                    return;
            }


            if (isMoving == false) {
                balls[cur_ball_index].GetComponent<TrailRenderer>().Clear();
                isMoving = true;
            }

            // Double click realisation
            if (Time.time - firstClickTime <= 0.3f) {
                if (direction == 1) {
                    curposition = 0;
                    balls[cur_ball_index].transform.position = positions[cur_ball_index][curposition];
                    isMoving = false;
                }
                else {
                    curposition = positions[cur_ball_index].Count - 1;
                    balls[cur_ball_index].transform.position = positions[cur_ball_index][curposition];
                    isMoving = false;
                }

                balls[cur_ball_index].GetComponent<TrailRenderer>().Clear();
            }
            else
                firstClickTime = Time.time;
        }
    }


    // Calculate coordinates_ball1 to positions_ball1
    void CoordinatesToPositions(Coordinates coordinates, out List<Vector3> positions) {
        positions = new List<Vector3>();
        for (int i = 0; i < coordinates.x.Capacity; ++i) {
            positions.Add(new Vector3((float)coordinates.x[i], (float)coordinates.y[i], (float)coordinates.z[i]));
        }
    }


    // On Slider's value changed
    void ChangeSpeed() {
        speed = SpeedSlider.value;
    }


    // Reset current ball
    void ResetBall(GameObject ball, List<Vector3> positions) {
        ball.transform.position = positions[0];
        ball.GetComponent<TrailRenderer>().Clear();
        direction = 1;
        curposition = 0;
        isMoving = false;
    }


    // Check camera parent
    void CheckCameraParent() {
        Camera.main.gameObject.transform.SetParent(balls[cur_ball_index].transform);
        gameObject.transform.localPosition = CAMERA_LOCAL_POSITION;
        gameObject.transform.LookAt(balls[cur_ball_index].transform);
    }


    // Controls retarget between balls
    void CheckRetargetBall() {
        if(Input.GetKeyDown(KeyCode.LeftArrow)) {
            ResetBall(balls[cur_ball_index], positions[cur_ball_index]);
            cur_ball_index += 1;
            if (cur_ball_index > 3)
                cur_ball_index = 0;
            CheckCameraParent();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            ResetBall(balls[cur_ball_index], positions[cur_ball_index]);
            cur_ball_index -= 1;
            if (cur_ball_index < 0)
                cur_ball_index = 3;
            CheckCameraParent();
        }
    }


    // Check input on camera rotation
    void CheckInputOnCameraRotation() {
        if (Input.GetMouseButton(1)) {
            gameObject.transform.LookAt(balls[cur_ball_index].transform);
            gameObject.transform.RotateAround(balls[cur_ball_index].transform.position, Vector3.up, Input.GetAxis("Mouse X") * 5);
        }
    }
}
