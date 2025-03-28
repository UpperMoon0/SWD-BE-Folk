﻿using System.ComponentModel.DataAnnotations;

namespace GrowthTracking.ShareLibrary.Abstract
{
    public abstract class BaseEntity
    {
        [MaxLength(100)]
        public virtual Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsDeleted { get; set; }

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.Now;
            UpdatedDate = DateTime.Now;
            IsDeleted = false;
        }
    }
}
