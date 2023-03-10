using System.Collections.Generic;
using UnityEngine;

namespace Universe
{
	internal class RemotePlayerSession
	{
		private readonly List<DebugReport> m_ReportList = new();

		/// <summary>
		/// 用户ID
		/// </summary>
		public int PlayerId { get; }

		/// <summary>
		/// 保存的报告最大数量
		/// </summary>
		public int MaxReportCount { get; }

		public int MinRangeValue
		{
			get
			{
				return 0;
			}
		}
		public int MaxRangeValue
		{
			get
			{
				int index = m_ReportList.Count - 1;
				if (index < 0)
					index = 0;
				return index;
			}
		}


		public RemotePlayerSession(int playerId, int maxReportCount = 1000)
		{
			PlayerId = playerId;
			MaxReportCount = maxReportCount;
		}

		/// <summary>
		/// 清理缓存数据
		/// </summary>
		public void ClearDebugReport()
		{
			m_ReportList.Clear();
		}

		/// <summary>
		/// 添加一个调试报告
		/// </summary>
		public void AddDebugReport(DebugReport report)
		{
			if (report == null)
				Debug.LogWarning("Invalid debug report data !");

			if (m_ReportList.Count >= MaxReportCount)
				m_ReportList.RemoveAt(0);
			m_ReportList.Add(report);
		}

		/// <summary>
		/// 获取调试报告
		/// </summary>
		public DebugReport GetDebugReport(int rangeIndex)
		{
			if (m_ReportList.Count == 0)
				return null;
			if (rangeIndex < 0 || rangeIndex >= m_ReportList.Count)
				return null;
			return m_ReportList[rangeIndex];
		}

		/// <summary>
		/// 规范索引值
		/// </summary>
		public int ClampRangeIndex(int rangeIndex)
		{
			if (rangeIndex < 0)
				return 0;

			if (rangeIndex > MaxRangeValue)
				return MaxRangeValue;

			return rangeIndex;
		}
	}
}