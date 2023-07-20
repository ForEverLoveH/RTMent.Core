using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RTMent.Core.GameSystem.GameHelper 
{

    #region 导入库函数
    [SuppressUnmanagedCodeSecurity]
    public  class SafeNativeMethods
    {
        // 创建一个libvlc实例，它是引用计数的
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr libvlc_new(int argc, IntPtr argv);

        // 释放libvlc实例
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void libvlc_release(IntPtr libvlc_instance);

        //获取libvlc的版本
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern String libvlc_get_version();

        //从视频来源(例如http、rtsp)构建一个libvlc_meida
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr libvlc_media_new_location(IntPtr libvlc_instance, IntPtr path);

        //从本地文件路径构建一个libvlc_media
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr libvlc_media_new_path(IntPtr libvlc_instance, IntPtr path);

        //释放libvlc_media
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void libvlc_media_release(IntPtr libvlc_media_inst);

        // 创建一个空的播放器
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr libvlc_media_player_new(IntPtr libvlc_instance);

        //从libvlc_media构建播放器
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr libvlc_media_player_new_from_media(IntPtr libvlc_media);

        //释放播放器资源
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void libvlc_media_player_release(IntPtr libvlc_mediaplayer);

        // 将视频(libvlc_media)绑定到播放器上
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void libvlc_media_player_set_media(IntPtr libvlc_media_player, IntPtr libvlc_media);

        // 设置图像输出的窗口
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void libvlc_media_player_set_hwnd(IntPtr libvlc_mediaplayer, Int32 drawable);

        //播放器播放
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int libvlc_media_player_play(IntPtr libvlc_mediaplayer);

        //播放器暂停
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void libvlc_media_player_pause(IntPtr libvlc_mediaplayer);

        //播放器停止
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void libvlc_media_player_stop(IntPtr libvlc_mediaplayer);

        // 解析视频资源的媒体信息(如时长等)
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void libvlc_media_parse(IntPtr libvlc_media);

        // 返回视频的时长(必须先调用libvlc_media_parse之后，该函数才会生效)
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern Int64 libvlc_media_get_duration(IntPtr libvlc_media);

        // 当前播放时间
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern Int64 libvlc_media_player_get_time(IntPtr libvlc_mediaplayer);

        // 设置播放时间
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void libvlc_media_player_set_time(IntPtr libvlc_mediaplayer, Int64 time);

        // 获取音量
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int libvlc_audio_get_volume(IntPtr libvlc_media_player);

        //设置音量
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void libvlc_audio_set_volume(IntPtr libvlc_media_player, int volume);

        // 设置全屏
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void libvlc_set_fullscreen(IntPtr libvlc_media_player, int isFullScreen);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int libvlc_get_fullscreen(IntPtr libvlc_media_player);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void libvlc_toggle_fullscreen(IntPtr libvlc_media_player);

        //判断播放时是否在播放
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern bool libvlc_media_player_is_playing(IntPtr libvlc_media_player);

        //判断播放时是否能够Seek
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern bool libvlc_media_player_is_seekable(IntPtr libvlc_media_player);

        //判断播放时是否能够Pause
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern bool libvlc_media_player_can_pause(IntPtr libvlc_media_player);

        //判断播放器是否可以播放
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int libvlc_media_player_will_play(IntPtr libvlc_media_player);

        //进行快照
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int libvlc_video_take_snapshot(IntPtr libvlc_media_player, int num, char[] filepath, int i_width, int i_height);


        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int libvlc_video_take_snapshot(IntPtr libvlc_media_player, int num, IntPtr filepath, int i_width, int i_height);

        //获取Media信息
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr libvlc_media_player_get_media(IntPtr libvlc_media_player);

        //获取媒体信息
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int libvlc_media_get_stats(IntPtr libvlc_media, ref libvlc_media_stats_t lib_vlc_media_stats);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void libvlc_media_add_option(IntPtr p_md, String psz_option);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Ansi)]
        //录制视频
        public static extern int libvlc_media_player_recorder_start(IntPtr libvlc_media_player, IntPtr dir, IntPtr path);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        //停止录制视频
        public static extern int libvlc_media_player_recorder_stop(IntPtr libvlc_media_player);
    }
    #endregion
}
