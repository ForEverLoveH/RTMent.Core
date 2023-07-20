
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTMent.Core.GameSystem.GameHelper
{

    public class MediaHelper : Singleton<MediaHelper>
    {

        #region 全局变量
        //数组转换为指针
        internal struct PointerToArrayOfPointerHelper
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public IntPtr[] pointers;
        }

        //vlc库启动参数配置
        private static string pluginPath = System.Environment.CurrentDirectory + "\\plugins\\";
        private static string plugin_arg = "--plugin-path=" + pluginPath;
        //用于播放节目时，转录节目
        private static string[] arguments = { "-I", "dummy", "--ignore-config", "--no-video-title",
            "--network-caching=110", plugin_arg };

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public  IntPtr Create_Media_Instance()
        {
            IntPtr libvlc_instance = IntPtr.Zero;
            IntPtr argvPtr = IntPtr.Zero;

            /*try
            {*/
            if (arguments.Length == 0 ||
                arguments == null)
            {
                return IntPtr.Zero;
            }

            //将string数组转换为指针
            argvPtr = StrToIntPtr(arguments);
            if (argvPtr == null || argvPtr == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            //设置启动参数
            libvlc_instance = SafeNativeMethods.libvlc_new(arguments.Length, argvPtr);
            if (libvlc_instance == null || libvlc_instance == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            return libvlc_instance;
            /* }
             catch (Exception ex)
             {
                 Console.WriteLine(ex.ToString());
                 return IntPtr.Zero;
             }*/
        }

        /// <summary>
        /// 释放VLC播放资源索引
        /// </summary>
        /// <param name="libvlc_instance">VLC 全局变量</param>
        public  void Release_Media_Instance(IntPtr libvlc_instance)
        {

            try
            {
                if (libvlc_instance != IntPtr.Zero ||
                    libvlc_instance != null)
                {
                    SafeNativeMethods.libvlc_release(libvlc_instance);
                }

                libvlc_instance = IntPtr.Zero;
            }
            catch (Exception)
            {
                libvlc_instance = IntPtr.Zero;
            }

        }

        /// <summary>
        /// 创建VLC播放器
        /// </summary>
        /// <param name="libvlc_instance">VLC 全局变量</param>
        /// <param name="handle">VLC MediaPlayer需要绑定显示的窗体句柄</param>
        /// <returns></returns>
        public  IntPtr Create_MediaPlayer(IntPtr libvlc_instance, IntPtr handle)
        {
            IntPtr libvlc_media_player = IntPtr.Zero;

            try
            {
                if (libvlc_instance == IntPtr.Zero ||
                    libvlc_instance == null ||
                    handle == IntPtr.Zero ||
                    handle == null)
                {
                    return IntPtr.Zero;
                }

                //创建播放器
                libvlc_media_player = SafeNativeMethods.libvlc_media_player_new(libvlc_instance);
                if (libvlc_media_player == null || libvlc_media_player == IntPtr.Zero)
                {
                    return IntPtr.Zero;
                }

                //设置播放窗口            
                SafeNativeMethods.libvlc_media_player_set_hwnd(libvlc_media_player, (int)handle);

                return libvlc_media_player;
            }
            catch
            {
                SafeNativeMethods.libvlc_media_player_release(libvlc_media_player);

                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// 视频存储开始
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <param name="url"></param>
        public  void Save_MediaPlayer(IntPtr libvlc_media_player, string dir, string fileName)
        {
            IntPtr pMrl = IntPtr.Zero;

            IntPtr pMr2 = IntPtr.Zero;

            try
            {
                if (dir == null ||
                    libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return;
                }

                pMrl = StrToIntPtr(dir);
                if (pMrl == null || pMrl == IntPtr.Zero)
                {
                    return;
                }
                pMr2 = StrToIntPtr(fileName);
                if (pMrl == null || pMrl == IntPtr.Zero)
                {
                    return;
                }

                SafeNativeMethods.libvlc_media_player_recorder_start(libvlc_media_player, pMrl, pMr2);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 停止录像功能，存储
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        public  void UnSave_MediaPlayer(IntPtr libvlc_media_player)
        {
            try
            {
                if (libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return;
                }
                SafeNativeMethods.libvlc_media_player_recorder_stop(libvlc_media_player);
            }
            catch (Exception)
            {

                throw;
            }
        }


        /// <summary>
        /// 释放媒体播放器
        /// </summary>
        /// <param name="libvlc_media_player">VLC MediaPlayer变量</param>
        public  void Release_MediaPlayer(IntPtr libvlc_media_player)
        {
            try
            {
                if (libvlc_media_player != null && libvlc_media_player != IntPtr.Zero)
                {
                    if (SafeNativeMethods.libvlc_media_player_is_playing(libvlc_media_player))
                    {
                        SafeNativeMethods.libvlc_media_player_stop(libvlc_media_player);
                    }

                    SafeNativeMethods.libvlc_media_player_release(libvlc_media_player);
                }

                libvlc_media_player = IntPtr.Zero;
            }
            catch (Exception)
            {
                libvlc_media_player = IntPtr.Zero;
            }
        }

        /// <summary>
        /// 播放网络媒体
        /// </summary>
        /// <param name="libvlc_instance">VLC 全局变量</param>
        /// <param name="libvlc_media_player">VLC MediaPlayer变量</param>
        /// <param name="url">网络视频URL，支持http、rtp、udp等格式的URL播放</param>
        /// <returns></returns>
        public  bool NetWork_Media_Play(IntPtr libvlc_instance, IntPtr libvlc_media_player, string url)
        {
            IntPtr pMrl = IntPtr.Zero;
            IntPtr libvlc_media = IntPtr.Zero;

            try
            {
                if (url == null ||
                    libvlc_instance == IntPtr.Zero ||
                    libvlc_instance == null ||
                    libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return false;
                }

                pMrl = StrToIntPtr(url);
                if (pMrl == null || pMrl == IntPtr.Zero)
                {
                    return false;
                }

                //播放网络文件
                libvlc_media = SafeNativeMethods.libvlc_media_new_location(libvlc_instance, pMrl);

                if (libvlc_media == null || libvlc_media == IntPtr.Zero)
                {
                    return false;
                }

                //将Media绑定到播放器上
                SafeNativeMethods.libvlc_media_player_set_media(libvlc_media_player, libvlc_media);

                //释放libvlc_media资源
                SafeNativeMethods.libvlc_media_release(libvlc_media);
                libvlc_media = IntPtr.Zero;

                if (0 != SafeNativeMethods.libvlc_media_player_play(libvlc_media_player))
                {
                    return false;
                }

                //休眠指定时间
                Thread.Sleep(200);

                return true;
            }
            catch (Exception)
            {
                //释放libvlc_media资源
                if (libvlc_media != IntPtr.Zero)
                {
                    SafeNativeMethods.libvlc_media_release(libvlc_media);
                }
                libvlc_media = IntPtr.Zero;

                return false;
            }
        }

        /// <summary>
        /// 播放本地媒体
        /// </summary>
        /// <param name="libvlc_instance">VLC 全局变量</param>
        /// <param name="libvlc_media_player">VLC MediaPlayer变量</param>
        /// <param name="url">本地文件路径</param>
        /// <returns></returns>
        public  bool LocationPath_Media_Play(IntPtr libvlc_instance, IntPtr libvlc_media_player, string url)
        {
            IntPtr pMrl = IntPtr.Zero;
            IntPtr libvlc_media = IntPtr.Zero;

            try
            {
                if (url == null ||
                    libvlc_instance == IntPtr.Zero ||
                    libvlc_instance == null ||
                    libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return false;
                }

                pMrl = StrToIntPtr(url);
                if (pMrl == null || pMrl == IntPtr.Zero)
                {
                    return false;
                }

                //播放网络文件
                libvlc_media = SafeNativeMethods.libvlc_media_new_path(libvlc_instance, pMrl);

                if (libvlc_media == null || libvlc_media == IntPtr.Zero)
                {
                    return false;
                }

                //将Media绑定到播放器上
                SafeNativeMethods.libvlc_media_player_set_media(libvlc_media_player, libvlc_media);

                //释放libvlc_media资源
                SafeNativeMethods.libvlc_media_release(libvlc_media);
                libvlc_media = IntPtr.Zero;

                if (0 != SafeNativeMethods.libvlc_media_player_play(libvlc_media_player))
                {
                    return false;
                }

                //休眠指定时间
                Thread.Sleep(200);

                return true;
            }
            catch (Exception)
            {
                //释放libvlc_media资源
                if (libvlc_media != IntPtr.Zero)
                {
                    SafeNativeMethods.libvlc_media_release(libvlc_media);
                }
                libvlc_media = IntPtr.Zero;

                return false;
            }
        }


        /// <summary>
        /// 暂停或恢复视频
        /// </summary>
        /// <param name="libvlc_media_player">VLC MediaPlayer变量</param>
        /// <returns></returns>
        public  bool MediaPlayer_Pause(IntPtr libvlc_media_player)
        {
            try
            {
                if (libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return false;
                }

                if (SafeNativeMethods.libvlc_media_player_can_pause(libvlc_media_player))
                {
                    SafeNativeMethods.libvlc_media_player_pause(libvlc_media_player);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        /// <param name="libvlc_media_player">VLC MediaPlayer变量</param>
        /// <returns></returns>
        public bool MediaPlayer_Stop(IntPtr libvlc_media_player)
        {
            try
            {
                if (libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return false;
                }

                SafeNativeMethods.libvlc_media_player_stop(libvlc_media_player);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 快进
        /// </summary>
        /// <param name="libvlc_media_player">VLC MediaPlayer变量</param>
        /// <returns></returns>
        public  bool MediaPlayer_Forward(IntPtr libvlc_media_player)
        {
            double time = 0;

            try
            {
                if (libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return false;
                }

                if (SafeNativeMethods.libvlc_media_player_is_seekable(libvlc_media_player))
                {
                    time = SafeNativeMethods.libvlc_media_player_get_time(libvlc_media_player) / 1000.0;
                    if (time == -1)
                    {
                        return false;
                    }

                    SafeNativeMethods.libvlc_media_player_set_time(libvlc_media_player, (Int64)((time + 30) * 1000));

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 快退
        /// </summary>
        /// <param name="libvlc_media_player">VLC MediaPlayer变量</param>
        /// <returns></returns>
        public  bool MediaPlayer_Back(IntPtr libvlc_media_player)
        {
            double time = 0;

            try
            {
                if (libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return false;
                }

                if (SafeNativeMethods.libvlc_media_player_is_seekable(libvlc_media_player))
                {
                    time = SafeNativeMethods.libvlc_media_player_get_time(libvlc_media_player) / 1000.0;
                    if (time == -1)
                    {
                        return false;
                    }

                    if (time - 30 < 0)
                    {
                        SafeNativeMethods.libvlc_media_player_set_time(libvlc_media_player, (Int64)(1 * 1000));
                    }
                    else
                    {
                        SafeNativeMethods.libvlc_media_player_set_time(libvlc_media_player, (Int64)((time - 30) * 1000));
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// VLC MediaPlayer是否在播放
        /// </summary>
        /// <param name="libvlc_media_player">VLC MediaPlayer变量</param>
        /// <returns></returns>
        public  bool MediaPlayer_IsPlaying(IntPtr libvlc_media_player)
        {
            try
            {
                if (libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return false;
                }

                return SafeNativeMethods.libvlc_media_player_is_playing(libvlc_media_player);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 录制快照
        /// </summary>
        /// <param name="libvlc_media_player">VLC MediaPlayer变量</param>
        /// <param name="path">快照要存放的路径</param>
        /// <param name="name">快照保存的文件名称</param>
        /// <returns></returns>
        public  bool TakeSnapShot(IntPtr libvlc_media_player, string path, string name)
        {
            try
            {
                string snap_shot_path = null;
                if (libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return false;
                }
                if(!File.Exists(path)) File.Create(path);
                snap_shot_path = path + "\\" + name;

                IntPtr IntPtrsnap_shot_path = StrToIntPtr(snap_shot_path);
                if (0 == SafeNativeMethods.libvlc_video_take_snapshot(libvlc_media_player, 0, IntPtrsnap_shot_path, 0, 0))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取信息
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <returns></returns>
        public  bool GetMedia(IntPtr libvlc_media_player)
        {
            IntPtr media = IntPtr.Zero;

            try
            {
                if (libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return false;
                }

                media = SafeNativeMethods.libvlc_media_player_get_media(libvlc_media_player);
                if (media == IntPtr.Zero || media == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取已经显示的图片数
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <returns></returns>
        public  int GetDisplayedPictures(IntPtr libvlc_media_player)
        {
            IntPtr media = IntPtr.Zero;
            libvlc_media_stats_t media_stats = new libvlc_media_stats_t();
            try
            {
                if (libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return 0;
                }

                media = SafeNativeMethods.libvlc_media_player_get_media(libvlc_media_player);
                if (media == IntPtr.Zero || media == null)
                {
                    return 0;
                }

                if (1 == SafeNativeMethods.libvlc_media_get_stats(media, ref media_stats))
                {
                    return media_stats.i_displayed_pictures;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// 设置全屏
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <param name="isFullScreen"></param>
        public  bool SetFullScreen(IntPtr libvlc_media_player, int isFullScreen)
        {
            try
            {
                if (libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return false;
                }

                SafeNativeMethods.libvlc_set_fullscreen(libvlc_media_player, isFullScreen);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region 私有函数
        //将string []转换为IntPtr
        public  IntPtr StrToIntPtr(string[] args)
        {
            try
            {
                IntPtr ip_args = IntPtr.Zero;

                PointerToArrayOfPointerHelper argv = new PointerToArrayOfPointerHelper();
                argv.pointers = new IntPtr[11];

                for (int i = 0; i < args.Length; i++)
                {
                    argv.pointers[i] = Marshal.StringToHGlobalAnsi(args[i]);
                }

                int size = Marshal.SizeOf(typeof(PointerToArrayOfPointerHelper));
                ip_args = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(argv, ip_args, false);

                return ip_args;
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        //将string转换为IntPtr
        private IntPtr StrToIntPtr(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    return IntPtr.Zero;
                }

                IntPtr pMrl = IntPtr.Zero;
                byte[] bytes = Encoding.UTF8.GetBytes(url);

                pMrl = Marshal.AllocHGlobal(bytes.Length + 1);
                Marshal.Copy(bytes, 0, pMrl, bytes.Length);
                Marshal.WriteByte(pMrl, bytes.Length, 0);

                return pMrl;
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }
        #endregion


    }
}
               