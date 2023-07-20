using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTMent.Core.GameSystem.GameHelper
{
    public struct libvlc_media_stats_t
    {
        /* Input */
        public int i_read_bytes;
        public float f_input_bitrate;

        /* Demux */
        public int i_demux_read_bytes;
        public float f_demux_bitrate;
        public int i_demux_corrupted;
        public int i_demux_discontinuity;

        /* Decoders */
        public int i_decoded_video;
        public int i_decoded_audio;

        /* Video Output */
        public int i_displayed_pictures;
        public int i_lost_pictures;

        /* Audio output */
        public int i_played_abuffers;
        public int i_lost_abuffers;

        /* Stream output */
        public int i_sent_packets;
        public int i_sent_bytes;
        public float f_send_bitrate;
    }
}
