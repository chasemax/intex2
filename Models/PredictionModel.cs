using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intex2.Models
{
    public class PredictionModel
    {
        public bool Work_Zone_Related { get; set; }
        public bool Pedestrian_Involved { get; set; }
        public bool Bicyclist_Involved { get; set; }
        public bool Motorcycle_Involved { get; set; }
        public bool Improper_Restraint { get; set; }
        public bool Unrestrained { get; set; }
        public bool DUI { get; set; }
        public bool Intersection_Related { get; set; }
        public bool Wild_Animal_Related { get; set; }
        public bool Domestic_Animal_Related { get; set; }
        public bool Overturn_Rollover { get; set; }
        public bool Commercial_Motor_Veh_Involved { get; set; }
        public bool Teenage_Driver_Involved { get; set; }
        public bool Older_Driver_Involved { get; set; }
        public bool Night_Dark_Condition { get; set; }
        public bool Single_Vehicle { get; set; }
        public bool Distracted_Driving { get; set; }
        public bool Drowsy_Driving { get; set; }
        public bool Roadway_Departure { get; set; }
    }
}
