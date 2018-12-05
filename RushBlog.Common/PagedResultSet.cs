using System.Collections.Generic;

namespace RushBlog.Common
{
    public class PagedResultSet<T>
    {
        #region Constants and Fields

        private int _endRecord;

        private int _startRecord;

        #endregion

        #region Constructors and Destructors

        public PagedResultSet()
        {
            PageResults = new List<T>();
            TotalRecords = 0;
            StartRecord = 0;
            EndRecord = 0;
        }

        #endregion

        #region Public Properties

        public int EndRecord
        {
            get
            {
                return _endRecord > TotalRecords ? TotalRecords : _endRecord;
            }

            set
            {
                _endRecord = value;
            }
        }

        public bool HasNextPage
        {
            get
            {
                return TotalRecords > EndRecord;
            }
            private set { }
        }

        public bool HasPreviousPage
        {
            get
            {
                return StartRecord - 1 > 0;
            }
            private set { }
        }

        public ICollection<T> PageResults { get; set; }

        public int StartRecord
        {
            get
            {
                return TotalRecords > 0 ? _startRecord : 0;
            }

            set
            {
                _startRecord = value;
            }
        }

        public int TotalRecords { get; set; }

        #endregion
    }
}
