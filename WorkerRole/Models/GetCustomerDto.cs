﻿using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerRole.Models
{
    public class GetCustomerDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
