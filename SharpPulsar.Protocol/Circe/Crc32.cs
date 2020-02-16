/// <summary>
/// Fast CRC32 Checksum
/// </summary>
namespace SharpPulsar.Protocol.Circe
{
	public class Crc32 : IChecksum
	{
		/// <summary>
		/// CRC32 from Ethernet/ANSI X3.66/ITU-T V.42
		/// <pre>
		/// The polynomial code used is 0x04C11DB7 (CRC32)
		/// x32+x26+x23+x22+x16+x12+x11+x10+x8+x7+x5+x4+x2+x+1 
		/// </pre>
		/// poly=0x04c11db7 init=0xffffffff refin=true refout=true xorout=0xffffffff
		/// @url http://en.wikipedia.org/wiki/Cyclic_redundancy_check
		/// @url http://reveng.sourceforge.net/crc-catalogue/17plus.htm
		/// </summary>
		private static readonly int[] CrcTable = new int[] { 0x00000000, 0x77073096, unchecked((int)0xEE0E612C), unchecked((int)0x990951BA), 0x076DC419, 0x706AF48F, unchecked((int)0xE963A535), unchecked((int)0x9E6495A3), 0x0EDB8832, 0x79DCB8A4, unchecked((int)0xE0D5E91E), unchecked((int)0x97D2D988), 0x09B64C2B, 0x7EB17CBD, unchecked((int)0xE7B82D07), unchecked((int)0x90BF1D91), 0x1DB71064, 0x6AB020F2, unchecked((int)0xF3B97148), unchecked((int)0x84BE41DE), 0x1ADAD47D, 0x6DDDE4EB, unchecked((int)0xF4D4B551), unchecked((int)0x83D385C7), 0x136C9856, 0x646BA8C0, unchecked((int)0xFD62F97A), unchecked((int)0x8A65C9EC), 0x14015C4F, 0x63066CD9, unchecked((int)0xFA0F3D63), unchecked((int)0x8D080DF5), 0x3B6E20C8, 0x4C69105E, unchecked((int)0xD56041E4), unchecked((int)0xA2677172), 0x3C03E4D1, 0x4B04D447, unchecked((int)0xD20D85FD), unchecked((int)0xA50AB56B), 0x35B5A8FA, 0x42B2986C, unchecked((int)0xDBBBC9D6), unchecked((int)0xACBCF940), 0x32D86CE3, 0x45DF5C75, unchecked((int)0xDCD60DCF), unchecked((int)0xABD13D59), 0x26D930AC, 0x51DE003A, unchecked((int)0xC8D75180), unchecked((int)0xBFD06116), 0x21B4F4B5, 0x56B3C423, unchecked((int)0xCFBA9599), unchecked((int)0xB8BDA50F), 0x2802B89E, 0x5F058808, unchecked((int)0xC60CD9B2), unchecked((int)0xB10BE924), 0x2F6F7C87, 0x58684C11, unchecked((int)0xC1611DAB), unchecked((int)0xB6662D3D), 0x76DC4190, 0x01DB7106, unchecked((int)0x98D220BC), unchecked((int)0xEFD5102A), 0x71B18589, 0x06B6B51F, unchecked((int)0x9FBFE4A5), unchecked((int)0xE8B8D433), 0x7807C9A2, 0x0F00F934, unchecked((int)0x9609A88E), unchecked((int)0xE10E9818), 0x7F6A0DBB, 0x086D3D2D, unchecked((int)0x91646C97), unchecked((int)0xE6635C01), 0x6B6B51F4, 0x1C6C6162, unchecked((int)0x856530D8), unchecked((int)0xF262004E), 0x6C0695ED, 0x1B01A57B, unchecked((int)0x8208F4C1), unchecked((int)0xF50FC457), 0x65B0D9C6, 0x12B7E950, unchecked((int)0x8BBEB8EA), unchecked((int)0xFCB9887C), 0x62DD1DDF, 0x15DA2D49, unchecked((int)0x8CD37CF3), unchecked((int)0xFBD44C65), 0x4DB26158, 0x3AB551CE, unchecked((int)0xA3BC0074), unchecked((int)0xD4BB30E2), 0x4ADFA541, 0x3DD895D7, unchecked((int)0xA4D1C46D), unchecked((int)0xD3D6F4FB), 0x4369E96A, 0x346ED9FC, unchecked((int)0xAD678846), unchecked((int)0xDA60B8D0), 0x44042D73, 0x33031DE5, unchecked((int)0xAA0A4C5F), unchecked((int)0xDD0D7CC9), 0x5005713C, 0x270241AA, unchecked((int)0xBE0B1010), unchecked((int)0xC90C2086), 0x5768B525, 0x206F85B3, unchecked((int)0xB966D409), unchecked((int)0xCE61E49F), 0x5EDEF90E, 0x29D9C998, unchecked((int)0xB0D09822), unchecked((int)0xC7D7A8B4), 0x59B33D17, 0x2EB40D81, unchecked((int)0xB7BD5C3B), unchecked((int)0xC0BA6CAD), unchecked((int)0xEDB88320), unchecked((int)0x9ABFB3B6), 0x03B6E20C, 0x74B1D29A, unchecked((int)0xEAD54739), unchecked((int)0x9DD277AF), 0x04DB2615, 0x73DC1683, unchecked((int)0xE3630B12), unchecked((int)0x94643B84), 0x0D6D6A3E, 0x7A6A5AA8, unchecked((int)0xE40ECF0B), unchecked((int)0x9309FF9D), 0x0A00AE27, 0x7D079EB1, unchecked((int)0xF00F9344), unchecked((int)0x8708A3D2), 0x1E01F268, 0x6906C2FE, unchecked((int)0xF762575D), unchecked((int)0x806567CB), 0x196C3671, 0x6E6B06E7, unchecked((int)0xFED41B76), unchecked((int)0x89D32BE0), 0x10DA7A5A, 0x67DD4ACC, unchecked((int)0xF9B9DF6F), unchecked((int)0x8EBEEFF9), 0x17B7BE43, 0x60B08ED5, unchecked((int)0xD6D6A3E8), unchecked((int)0xA1D1937E), 0x38D8C2C4, 0x4FDFF252, unchecked((int)0xD1BB67F1), unchecked((int)0xA6BC5767), 0x3FB506DD, 0x48B2364B, unchecked((int)0xD80D2BDA), unchecked((int)0xAF0A1B4C), 0x36034AF6, 0x41047A60, unchecked((int)0xDF60EFC3), unchecked((int)0xA867DF55), 0x316E8EEF, 0x4669BE79, unchecked((int)0xCB61B38C), unchecked((int)0xBC66831A), 0x256FD2A0, 0x5268E236, unchecked((int)0xCC0C7795), unchecked((int)0xBB0B4703), 0x220216B9, 0x5505262F, unchecked((int)0xC5BA3BBE), unchecked((int)0xB2BD0B28), 0x2BB45A92, 0x5CB36A04, unchecked((int)0xC2D7FFA7), unchecked((int)0xB5D0CF31), 0x2CD99E8B, 0x5BDEAE1D, unchecked((int)0x9B64C2B0), unchecked((int)0xEC63F226), 0x756AA39C, 0x026D930A, unchecked((int)0x9C0906A9), unchecked((int)0xEB0E363F), 0x72076785, 0x05005713, unchecked((int)0x95BF4A82), unchecked((int)0xE2B87A14), 0x7BB12BAE, 0x0CB61B38, unchecked((int)0x92D28E9B), unchecked((int)0xE5D5BE0D), 0x7CDCEFB7, 0x0BDBDF21, unchecked((int)0x86D3D2D4), unchecked((int)0xF1D4E242), 0x68DDB3F8, 0x1FDA836E, unchecked((int)0x81BE16CD), unchecked((int)0xF6B9265B), 0x6FB077E1, 0x18B74777, unchecked((int)0x88085AE6), unchecked((int)0xFF0F6A70), 0x66063BCA, 0x11010B5C, unchecked((int)0x8F659EFF), unchecked((int)0xF862AE69), 0x616BFFD3, 0x166CCF45, unchecked((int)0xA00AE278), unchecked((int)0xD70DD2EE), 0x4E048354, 0x3903B3C2, unchecked((int)0xA7672661), unchecked((int)0xD06016F7), 0x4969474D, 0x3E6E77DB, unchecked((int)0xAED16A4A), unchecked((int)0xD9D65ADC), 0x40DF0B66, 0x37D83BF0, unchecked((int)0xA9BCAE53), unchecked((int)0xDEBB9EC5), 0x47B2CF7F, 0x30B5FFE9, unchecked((int)0xBDBDF21C), unchecked((int)0xCABAC28A), 0x53B39330, 0x24B4A3A6, unchecked((int)0xBAD03605), unchecked((int)0xCDD70693), 0x54DE5729, 0x23D967BF, unchecked((int)0xB3667A2E), unchecked((int)0xC4614AB8), 0x5D681B02, 0x2A6F2B94, unchecked((int)0xB40BBE37), unchecked((int)0xC30C8EA1), 0x5A05DF1B, 0x2D02EF8D };

		private int _crc = ~0;

		public virtual void Update(int d)
		{
			_crc = (((int)((uint)_crc >> 8)) ^ CrcTable[(_crc ^ (d & 0xFF)) & 0xFF]);
		}

		public virtual void Update(sbyte[] buffer, int offset, int length)
		{
			for (int i = offset; length > 0; length--)
			{
				Update(buffer[i++]);
			}
		}
		public virtual void Update(sbyte[] buffer)
		{
			for (int i = 0; i < buffer.Length; i++)
			{
				Update(buffer[i]);
			}
		}

		public virtual long Value => ((long)(_crc ^ 0xFFFFFFFF) & 0xFFFFFFFFL);

        public long GetValue()
        {
            return Value;
        }

        public virtual void Reset()
		{
			_crc = ~0;
		}

	}
}
