using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("AccountFamilyMembers")]
    public class AccountFamilyMember
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AccountId { get; set; }

        [ForeignKey("AccountId")]
        public Account Account { get; set; }

        public Guid? FlyJestMemberId { get; set; }

        [ForeignKey("FlyJestMemberId")]
        public Account FlyJestMember { get; set; }

        [Column(TypeName = "NVARCHAR(30)")]
        public string FirstName { get; set; }

        [Column(TypeName = "NVARCHAR(30)")]
        public string LastName { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? DateOfBirth { get; set; }

        [Column(TypeName = "NVARCHAR(100)")]
        public string Email { get; set; }

        [Column(TypeName = "NVARCHAR(250)")]
        public string Address { get; set; }

        [Column(TypeName = "NVARCHAR(30)")]
        public string Mobile { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedOn { get; set; }
    }
}
