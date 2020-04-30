using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dojo_Activity.Models
{
    public class IndexViewModel
    {
        public LoginUser LoggedUser {get;set;}
        public User RegisteredUser {get;set;}
    }
}