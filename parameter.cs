using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uart_recv
{
    class parameter
    {
    }
    class SerialData
    {
        public List<byte> buffer = new List<byte>(4096);
        public UInt16 index = 0;
    }



    class BeijingTime
    {
        public int year;
        public int month;
        public int day;
        public int hour;
        public int min;
        public int sec;
        public int msec;
        public string time;
    }



    class reciveData
    {
        public byte[] command_receive = new byte[77];
        public int receive_count;       // 实际接收到的帧计数
        public int count;               // 数据包中的帧计数

        public double ax     ;     public double ax_1s     ;
        public double ay     ;     public double ay_1s     ;
        public double az     ;     public double az_1s     ;
        public double wx     ;     public double wx_1s     ;
        public double wy     ;     public double wy_1s     ;
        public double wz     ;     public double wz_1s     ;
        public double a_tem  ;     public double a_tem_1s  ;
        public double wx_tem ;     public double wx_tem_1s ;
        public double wy_tem ;     public double wy_tem_1s ;
        public double wz_tem ;     public double wz_tem_1s ;

        public double lat    ;
        public double lon    ;
        public double alt    ;
        public double pitch  ;
        public double roll   ;
        public double yaw    ;
        public double vx     ;
        public double vy     ;
        public double vz     ;

        public double exint;
        public double eyint;
        public double ezint;

        public int num;
        public int showed_index;


    }


    class compsys
    {
        public double    fogx_bias;
        public double    fogy_bias;
        public double    fogz_bias;
        public double    accx_bias;
        public double    accy_bias;
        public double    accz_bias;

        public double fog11,fog12,fog13;
        public double fog21,fog22,fog23;
        public double fog31,fog32,fog33;

        public double acc11, acc12, acc13;
        public double acc21, acc22, acc23;
        public double acc31, acc32, acc33;

        public double fogx_lp_xz;
        public double fogy_lp_xz;
        public double fogz_lp_xz;
        public double accx_lp_xz;
        public double accy_lp_xz;
        public double accz_lp_xz;

    }






    class ConfigInf
    {

        /****************************************校验配置****************************************/
        public int totalbytes;                     // 总字节数

        public int header_startloc;
        public int header_bytes;
        public byte[] header = new byte[10];        // 考虑到需留出余量

        // 固定数据帧尾
        public int tail1_startloc;
        public int tail1_bytes;
        public byte[] tail = new byte[10];

        // 校验和帧尾
        public int tail2_startloc;
        public int tail2_bytes;
        public int tail2_check_startloc;
        public int tail2_check_endloc;

        // CRC校验帧尾
        public int tail3_startloc;
        public int tail3_bytes;
        public int tail3_check_startloc;
        public int tail3_check_endloc;

        /***************************************解码配置****************************************/

    }

}
