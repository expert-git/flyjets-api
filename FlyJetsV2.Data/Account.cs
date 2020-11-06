using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("Accounts")]
    public class Account
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "VARCHAR(30)")]
        public string Number { get; set; }

        [Required]
        public byte Type { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(100)")]
        public string Email { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(300)")]
        public string Password { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(30)")]
        public string FirstName { get; set; }

        [Column(TypeName = "NVARCHAR(30)")]
        public string MiddleName { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(30)")]
        public string LastName { get; set; }

        [Column(TypeName = "NVARCHAR(30)")]
        public string Mobile { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedOn { get; set; }

        [Column(TypeName = "NVARCHAR(250)")]
        public string Address { get; set; }
        public string ImageFileName { get; set; }

        public byte? TitleId { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [NotMapped]
        public string ThumbnailImageFileName
        {
            get
            {
                return string.IsNullOrEmpty(ImageFileName) ? "" : "thumb_" + ImageFileName;
            }
        }

        public byte? NotificationsChannel { get; set; }

        [Column(TypeName = "NVARCHAR(120)")]
        public string CompanyName { get; set; }

        [Column(TypeName = "NVARCHAR(250)")]
        public string CompanyAddress { get; set; }

        [Column(TypeName = "NVARCHAR(100)")]
        public string CompanyEmail { get; set; }

        [Column(TypeName = "NVARCHAR(30)")]
        public string CompanyPhone { get; set; }

        public byte Status { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? LastLogInDate { get; set; }

        public Guid? CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public Account CreatedBy { get; set; }

        public decimal? TotalFlyRewards { get; set; }

        public short? TotalFlyRewardsPoints { get; set; }

        [Column(TypeName = "NVARCHAR(120)")]
        public string VerificationCode { get; set; }

        [Column(TypeName = "NVARCHAR(120)")]
        public string ResetPasswordCode { get; set; }

        public string StripeCustomerId { get; set; }

        public bool ManagedByFlyJets { get; set; }

        public bool PasswordExpired { get; set; }
    }
}
