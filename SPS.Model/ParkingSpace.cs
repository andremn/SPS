﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool
//     Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SPS.Model
{
    public class ParkingSpace
    {
        [Key]
        public virtual int Number
        {
            get;
            set;
        }

        public virtual ParkingSpaceState Status
        {
            get;
            set;
        }

        public virtual Parking Parking
        {
            get;
            set;
        }
    }
}
