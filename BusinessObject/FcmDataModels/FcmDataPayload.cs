using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Enums;
using System.Linq;
namespace BusinessObject.FcmDataModels
{
    public class FcmDataPayload
    {
        public NotificationType Type { get; set; }
        public AppScreen? Screen { get; set; }

        public EntityKeyType? EntityKey { get; set; } 
        public Guid? EntityId { get; set; }            
        public Dictionary<string, string>? ExtraData { get; set; } // tuỳ chọn

        
        public string? Title { get; set; }
        public string? Body { get; set; }

        public Dictionary<string, string> ToDictionary()
        {
            var dict = new Dictionary<string, string>
            {
                ["type"] = Type.ToString().ToLower()
            };

            if (!string.IsNullOrEmpty(Title))
                dict["title"] = Title;

            if (!string.IsNullOrEmpty(Body))
                dict["body"] = Body;

            if (EntityKey.HasValue && EntityId.HasValue)
            {
                string key = ConvertEnumToKey(EntityKey.Value);
                dict[key] = EntityId.Value.ToString();
            }

            if (Screen.HasValue)
                dict["screen"] = Screen.Value.ToString();

            if (ExtraData != null)
            {
                foreach (var kv in ExtraData)
                    dict[kv.Key] = kv.Value;
            }

            return dict;
        }

        public static string ConvertEnumToKey(EntityKeyType key)
        {
            string str = key.ToString();
            if (string.IsNullOrEmpty(str))
                return str;

            // chữ đầu viết thường, giữ nguyên các chữ còn lại
            return char.ToLower(str[0]) + str.Substring(1);
        }
    }
}