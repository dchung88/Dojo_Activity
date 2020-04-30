using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dojo_Activity.Models
{
    public class Actvty
    {
        [Key]
        public int ActivityId {get;set;}

        [Required]
        [MinLength(2)]
        public string Title {get;set;}

        [Required]
        public string Description {get;set;}

        [Required]
        [FutureDateTime]
        [DataType(DataType.DateTime)]
        public DateTime ActivityDate {get;set;}

        [Required]
        public int ActDuration {get;set;}

        [Required]
        public string ActUnit {get;set;}

        public int PlannerId {get;set;}
        public List<Participation> Participants {get;set;}
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;
    }
}