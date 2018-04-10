using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TcpServcerTest
{
    public class ResultsStructure
    {
        private string _batchNo;

        public string BatchNo
        {
            get { return _batchNo; }
            set { _batchNo = value; }
        }
        private int _batchSeq;

        public int BatchSeq
        {
            get { return _batchSeq; }
            set { _batchSeq = value; }
        }
        private double _fmValue;

        public double FmValue
        {
            get { return _fmValue; }
            set { _fmValue = value; }
        }
        private double _feHValue;

        public double FeHValue
        {
            get { return _feHValue; }
            set { _feHValue = value; }
        }
        private double _feLValue;

        public double FeLValue
        {
            get { return _feLValue; }
            set { _feLValue = value; }
        }
        private double _luValue;

        public double LuValue
        {
            get { return _luValue; }
            set { _luValue = value; }
        }
        private double _fb02Value;

        public double Fb02Value
        {
            get { return _fb02Value; }
            set { _fb02Value = value; }
        }
        private double _rmValue;

        public double RmValue
        {
            get { return _rmValue; }
            set { _rmValue = value; }
        }
        private double _rp02Value;

        public double Rp02Value
        {
            get { return _rp02Value; }
            set { _rp02Value = value; }
        }

        private string fracturePosition;

        public string FracturePosition
            {
            get { return fracturePosition; }
            set { fracturePosition = value; }
            }

        private double a;

        public double A
            {
            get { return a; }
            set { a = value; }
            }
        }
}
