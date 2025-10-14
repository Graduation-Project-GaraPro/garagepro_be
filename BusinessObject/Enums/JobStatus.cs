using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Enums
{
    public enum JobStatus
    {
        Pending = 0,       //Lúc tạo Job nhưng Manager chưa gán cho Technician    
        New = 1,  	 //Manager tạo Job và gán cho Technician
        InProgress = 2,  //Technician đang làm
        Completed = 3,  //Technician đã hoàn thiện Job
        OnHold = 4          //Technician chờ phụ tùng hoặc gặp vấn đề khi  sửa chữa
    }
}
