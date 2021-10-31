using System.IO;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *  This Message class is serializable and capable of transmittal over PUN using PhotonNetwork.RaiseEvent
     */
    public class Message : ICommandData
    {
        public static readonly byte EventID = 42;

        public byte getEventID { get { return EventID; } }

        /*
         *      Default constructor for the Message class.  Sets the public class parameters to default (empty) values.
         *      
         *      Parameters
         *      ----------
         *      None
         */
        public Message()
        {
            this.Text = "";
        }

        /*
         *      Constructor for the Message class.
         *      
         *      Parameters
         *      ----------
         *      eventID : byte
         *          Corresponds to the EventID provided over the PUN network.
         *          
         *      text : string
         *          The text string to be sent with this message class.
         */
        public Message(string text)
        {
            this.Text = text;
        }

        /*
         *      Text string accessor property.
         *      
         *      Accessor
         *      -------
         *      text : string
         *          Get or set the text string to be sent with this message class.
         */
        public string Text
        {
            get; set;
        }

        /*
         *      Returns the Text property, which represents this Message class.
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
            return Text;
        }

        /*
         *      Converts this Message class into an object array of its properties.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      output : object[]
         *          An object array.  The first element is the EventID property, 
         *          and the second element is the Text property.
         */
        public object[] ToObjectArray()
        {
            object[] data = new object[2];
            data[0] = EventID;
            data[1] = Text;
            return data;
        }

        /*
         *      Reconstructs a Message class object from an Object array.
         *      This static method reverses the action taken by the 
         *      ToObjectArray() method.
         *      
         *      Parameters
         *      ----------
         *      data : object[]
         *          An object array.  The first element is the EventID property, 
         *          and the second element is the Text property.
         *      
         *      Returns
         *      -------
         *      output : Message
         *          A Message class object populated with data from the object
         *          array.
         */
        public static Message FromObjectArray(object[] data)
        {
            Message result = new Message();
            byte ID = (byte)data[0];
            if (ID == Message.EventID)
            {
                result.Text = (string)data[1];
                return result;
            }
            else
            {
                Debug.LogErrorFormat("Event ID {0} does not match", ID);
                return null;
            }
        }

        /*
         *      Converts this Message class into a byte array of its properties.  
         *      This is the most compressed representation possible.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      output : byte[]
         *          A byte array.  The first element is the EventID byte-type
         *          property, and the second element is the Text string-type
         *          property.
         */
        public byte[] ToByteArray()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(EventID);
                    writer.Write(Text);
                }
                return m.ToArray();
            }
        }

        /*
         *      Reconstructs a Message class object from a byte array.
         *      This static method reverses the action taken by the 
         *      ToByteArray() method.
         *      
         *      Parameters
         *      ----------
         *      data : byte[]
         *          A byte array.  The first element is the EventID byte-type
         *          property, and the second element is the Text string-type
         *          property.
         *      
         *      Returns
         *      -------
         *      output : Message
         *          A Message class object populated with data from the byte
         *          array.
         */
        public static Message FromByteArray(byte[] data)
        {
            Message result = new Message();
            using (MemoryStream m = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    byte EventID = reader.ReadByte();
                    if (EventID != Message.EventID)
                        Debug.LogError("Event ID does not match.");
                    result.Text = reader.ReadString();
                }
            }
            return result;
        }
    }
}
