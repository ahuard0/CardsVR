using System.IO;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *  This Movement class is serializable and capable of transmittal over PUN using PhotonNetwork.RaiseEvent.
     *  
     *  This class represents the position and rotation orientation of an object.
     */
    public class Movement : ICommandData
    {
        public static readonly byte EventID = 43;
        public enum Type { Absolute, Relative };

        public byte getEventID { get { return EventID; } }

        /*
         *      Constructor for the Movement class.
         *      
         *      Parameters
         *      ----------
         *      name : string
         *          The unique name of the GameObject being moved.  Required.
         *      pos : Vector3
         *          The new translation vector of the GameObject being moved.  If null, no translation is performed.  Default is (0,0,0).
         *      rot : Quaternion
         *          The new Euler angle vector of the GameObject being moved.  If null, no rotation is performed.  Default is (0,0,0).
         *      type : Type (enum)
         *          Defines whether the movement is to an absolute or relative position.  Default is Absolute.
         */
        public Movement()
        {
            this.Name = "";
            this.Pos = new Vector3(0, 0, 0);
            this.Rot = Quaternion.identity;
            this.type = Type.Absolute;
        }
        public Movement(string name)
        {
            this.Name = name;
            this.Pos = new Vector3(0, 0, 0);
            this.Rot = Quaternion.identity;
            this.type = Type.Absolute;
        }
        public Movement(string name, Vector3 pos)
        {
            this.Name = name;
            this.Pos = pos;
            this.Rot = Quaternion.identity;
            this.type = Type.Absolute;
        }
        public Movement(string name, Vector3 pos, Quaternion rot)
        {
            this.Name = name;
            this.Pos = pos;
            this.Rot = rot;
            this.type = Type.Absolute;
        }
        public Movement(string name, Vector3 pos, Quaternion rot, Type type)
        {
            this.Name = name;
            this.Pos = pos;
            this.Rot = rot;
            this.type = type;
        }
        public Movement(string name, Vector3 pos, Vector3 euler)
        {
            this.Name = name;
            this.Pos = pos;
            this.Euler = euler;
            this.type = Type.Absolute;
        }
        public Movement(string name, Vector3 pos, Vector3 euler, Type type)
        {
            this.Name = name;
            this.Pos = pos;
            this.Euler = euler;
            this.type = type;
        }

        /*
         *      Translation (position) accessor property.
         *      
         *      Accessor
         *      -------
         *      Pos : Vector3
         *          Get or set the new translation vector of the GameObject being moved.
         */
        public Vector3 Pos
        {
            get; set;
        }

        /*
         *      Rotation accessor property.
         *      
         *      Accessor
         *      -------
         *      Rot : Quaternion
         *          Get or set the new Quaternion rotation vector of the GameObject being moved.
         */
        public Quaternion Rot
        {
            get; set;
        }

        /*
         *      Rotation accessor property.
         *      
         *      Accessor
         *      -------
         *      Euler : Vector3
         *          Get or set the new Euler rotation vector of the GameObject being moved.
         */
        public Vector3 Euler
        {
            get 
            {
                return this.Rot.eulerAngles;
            }

            set
            {
                this.Rot = Quaternion.Euler(value);
            }
        }

        /*
         *      Movement type accessor property.
         *      
         *      Accessor
         *      -------
         *      type : Type
         *          Defines whether the movement is to an absolute or relative position.
         */
        public Type type
        {
            get; set;
        }

        /*
         *      Name string accessor property.
         *      
         *      Accessor
         *      -------
         *      Name : string
         *          Get or set the name of the GameObject being moved.
         */
        public string Name
        {
            get; set;
        }

        /*
         *      Returns the Name property, which represents this Movement class.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      output : string
         *          The text string to be sent with this message class.
         */
        public override string ToString()
        {
            return "Name: " + Name + ", Pos: " + Pos.ToString() + ", Rot: " + Rot.ToString() + ", " + type.ToString();
        }

        /*
         *      Converts this Movement class into an object array of its properties.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      output : object[]
         *          An object array.  The first element is the EventID property, 
         *          and the remaining elements are the Position, Rotation, and 
         *          type (enum) properties.
         */
        public object[] ToObjectArray()
        {
            object[] data = new object[5];
            data[0] = EventID;
            data[1] = Name;
            data[2] = Pos;
            data[3] = Rot;
            data[4] = type;
            return data;
        }

        /*
         *      Reconstructs a Movement class object from an Object array.
         *      This static method reverses the action taken by the 
         *      ToObjectArray() method.
         *      
         *      Parameters
         *      ----------
         *      data : object[]
         *          An object array.  The first element is the EventID property, 
         *          and the remaining elements are the Position, Rotation, and 
         *          type (enum) properties.
         *      
         *      Returns
         *      -------
         *      output : Movement
         *          A Movement class object populated with data from the object
         *          array.
         */
        public static Movement FromObjectArray(object[] data)
        {
            Movement result = new Movement();
            byte ID = (byte)data[0];
            if (ID == Movement.EventID)
            {
                result.Name = (string)data[1];
                result.Pos = (Vector3)data[2];
                result.Rot = (Quaternion)data[3];
                result.type = (Type)data[4];
                return result;
            }
            else
            {
                Debug.LogErrorFormat("Event ID {0} does not match", ID);
                return null;
            }
        }
    }
}
