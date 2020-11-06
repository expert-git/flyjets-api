using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlyJetsV2.Data
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "NVARCHAR(150)")]
        public string Text { get; set; }

        public Guid ReceiverId { get; set; }

        [ForeignKey("ReceiverId")]
        public Account Receiver { get; set; }

        public Guid? SenderId { get; set; }

        [ForeignKey("SenderId")]
        public Account Sender { get; set; }

        public bool Read { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedOn { get; set; }

        [Column(TypeName = "VARCHAR(max)")]
        public string Params { get; set; }
    }
}
