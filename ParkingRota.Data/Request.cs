﻿namespace ParkingRota.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Business.Model;
    using NodaTime;

    public class Request
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        public LocalDate Date
        {
            get => DbConvert.LocalDate.FromDb(this.DbDate);
            set => this.DbDate = DbConvert.LocalDate.ToDb(value);
        }

        [Required]
        public DateTime DbDate { get; set; }

        public bool IsAllocated { get; set; }
    }
}