﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Webshop.Models
{
    public class Basket
    {
        public int Id { get; set; } // Primary Key

        public int Quantity { get; set; }

        public int Customer { get; set; } // Foreign Key --> Customer(Id)
        public Customer CustomerObj { get; set; }

        public int Product { get; set; } // Foreign Key --> Product(Id)
        public Product ProductObj { get; set; } 
    }
}