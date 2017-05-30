using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class BallFlight : MonoBehaviour {

    // Class with coordinates
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
    const string FILENAME = @"C:\Andrey\Dev\Repositories\Unity\TestTask\Assets\Resources\ball_path.json";
    const int SPEED = 2;    // Flight speed
    int curspeed;            // Counter for flight speed
    int curposition;         // Current ball position
    int direction;           // Direction of ball moving
    bool isMoving;           // Is object moving  
    Coordinates coordinates; // List of coordinates from ball_path.json

    void Start () {
        coordinates= new Coordinates();
        curspeed = 0;
        curposition = 0;
        direction = 1;
        isMoving = false;

        ParseJsonCoordinates(ReadFileToString(FILENAME), out coordinates);

        gameObject.transform.position = new Vector3((float)coordinates.x[0], (float)coordinates.y[0], (float)coordinates.z[0]);
    }

    void Update () {
        // Check condition coordinates is empty
        if (coordinates == null || coordinates.x.Capacity == 0 ||
            coordinates.y.Capacity == 0 || coordinates.z.Capacity == 0)
            return;

		if (isMoving && curspeed++ == SPEED) {

            MoveBall(direction);

            // Check if ball reach the end point
            if (curposition >= coordinates.x.Capacity - 1 || curposition <= 0) {
                isMoving = false;
                direction *= -1;
            }

            curspeed = 0;
        }
	}

    // Read json file to Array of Lists
    bool ParseJsonCoordinates(string json_data, out Coordinates coordinates) {
        
        // Check if read string is empty
        if (json_data.Length == 0) {
            Debug.Log("BallFlight.ParseJsonCoordinates(): Json data is empty!");
            coordinates = null;
            return false;
        }

        coordinates = JsonUtility.FromJson<Coordinates>(json_data);
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
     * dorection == -1 ball moves backward
     */
    void MoveBall(int direction) {
        curposition += direction;
        gameObject.transform.position = new Vector3((float)coordinates.x[curposition], (float)coordinates.y[curposition], (float)coordinates.z[curposition]);
    }

    // If mouse clicked on object
    private void OnMouseDown() {
        if (isMoving == false)
            isMoving = true;
    }
}
