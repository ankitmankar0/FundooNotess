﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RepositoryLayer.Entity
{
    public class Note
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int NoteId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string BgColour { get; set; }

        public bool IsPin { get; set; }
        public bool IsArchieve { get; set; }
        public bool IsReminder { get; set; }
        public bool IsTrash { get; set; }

        public DateTime RegsterDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime ReminderDate { get; set; }

       
        [ForeignKey("User")]
        public int userID { get; set; }
        public virtual User User { get; set; }

        public virtual IList<Label> labels { get; set; }

    }
}
