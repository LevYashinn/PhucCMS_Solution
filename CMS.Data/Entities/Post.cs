
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Data.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl {  get; set; }
        public DateTime CreatedDate { get; set; } =  DateTime.Now;
        public int CategoryId {  get; set; }
        public virtual Category Category { get; set; }
    }
}
