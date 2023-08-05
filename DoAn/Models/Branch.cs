﻿using System;
using System.Collections.Generic;

namespace DoAn.Models;

public partial class Branch
{
    public int BranchId { get; set; }

    public string? Address { get; set; }

    public string? Hotline { get; set; }

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
}