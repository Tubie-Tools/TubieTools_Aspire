namespace TubieTools_Aspire.Tests.Algorithms
{
    [TestClass]
    public class UnitTestSorting
    {
        ISortingService service = new SortingService();
        // ramp this value up to 500,000 and watch certain algorithms fail to complete in a reasonable time frame
        readonly int[] data = new int[1000];

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestInitialize]
        public void Init()
        {
            Random r = new Random();
            IntArrayGenerate(data, r.Next(1, Int32.MaxValue));
        }

        private static void IntArrayGenerate(int[] data, int randomSeed)
        {
            Random r = new Random(randomSeed);
            for (int i = 0; i < data.Length; i++)
                data[i] = r.Next();
        }


        [TestMethod]
        public void TestSortChars()
        {
            char chA = 'A';
            char ch1 = '1';
            string str = "test string";

            char chF = 'F';
            TestContext.WriteLine(Char.GetNumericValue(chF).ToString());

            TestContext.WriteLine(chA.CompareTo('B').ToString());          //-----------  Output: "-1" (meaning 'A' is 1 less than 'B')
            TestContext.WriteLine(chA.Equals('A').ToString());             //-----------  Output: "True"
            TestContext.WriteLine(Char.GetNumericValue(ch1).ToString());   //-----------  Output: "1"
            TestContext.WriteLine(Char.IsControl('\t').ToString());        //-----------  Output: "True"
            TestContext.WriteLine(Char.IsDigit(ch1).ToString());           //-----------  Output: "True"
            TestContext.WriteLine(Char.IsLetter(',').ToString());          //-----------  Output: "False"
            TestContext.WriteLine(Char.IsLower('u').ToString());           //-----------  Output: "True"
            TestContext.WriteLine(Char.IsNumber(ch1).ToString());          //-----------  Output: "True"
            TestContext.WriteLine(Char.IsPunctuation('.').ToString());     //-----------  Output: "True"
            TestContext.WriteLine(Char.IsSeparator(str, 4).ToString());    //-----------  Output: "True"
            TestContext.WriteLine(Char.IsSymbol('+').ToString());          //-----------  Output: "True"
            TestContext.WriteLine(Char.IsWhiteSpace(str, 4).ToString());   //-----------  Output: "True"
            TestContext.WriteLine(Char.Parse("S").ToString());             //-----------  Output: "S"
            TestContext.WriteLine(Char.ToLower('M').ToString());           //-----------  Output: "m"
            TestContext.WriteLine('x'.ToString());
        }
        /// <summary>
        /// if index before and after are same, set to 0
        /// otherwise set to 1
        /// for ends, assume for index 0 that left is 0, and for index 8 right is 0
        /// assume they all change at the same time
        /// </summary>
        [TestMethod]
        public void TestCityArray()
        {
            int days = 1;
            int[] items = { 0, 1, 0, 0, 1, 0, 0 };
            int[] tempItems = new int[items.Length];

            for (int i = 0; i < days; i++)
            {
                for (int j = 0; j <= items.Length - 1; j++)
                {
                    if (j > 0 && j < items.Length - 1)
                    {
                        TestContext.WriteLine("found values                     j+1 =" + items[j + 1] + "and j-1=" + items[j - 1]);
                        if (items[j + 1] == items[j - 1])
                        {
                            tempItems[j] = 0;
                        }
                        else
                        {
                            tempItems[j] = 1;
                        }
                    }
                    else if (j == 0)
                    {
                        TestContext.WriteLine("found values at beginning of array items of j+1 =  " + items[j]);
                        // beginning of array assume left value is 0
                        if (items[j + 1] == 0)
                        {
                            tempItems[j] = 0;
                        }
                        else
                        {
                            tempItems[j] = 1;
                        }
                    }
                    else if (j == items.Length - 1)
                    {
                        TestContext.WriteLine("found values end of array items of j= " + items[j]);
                        // end of array asssume right value is 0
                        if (items[j] == 0)
                        {
                            tempItems[j] = 0;
                        }
                        else
                        {
                            tempItems[j] = 1;
                        }
                    }
                    else
                    {
                        TestContext.WriteLine("failed");
                    }
                }

                tempItems.CopyTo(items, 0);
                tempItems = new int[items.Length];

            }

            Assert.IsNotNull(items);

        }

        [TestMethod]
        public void QuickSort()
        {
            service.IntArrayQuickSort(data);
            Assert.IsNotNull(data);
        }



        [TestMethod]
        public void TestBubbleSort()
        {
            service.IntArrayBubbleSort(data);


            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestModifiedBubble()
        {
            // testing more about bubble sort
            service.ModifiedBubbleSort(data);
            //  | |
            // ( | )
            Assert.IsNotNull(data);
        }

        [Ignore]
        [TestMethod]
        public void TestSelectionSort()
        {
            service.IntArraySelectionSort(data);

            Assert.IsNotNull(data);
        }

        [Ignore]
        [TestMethod]
        public void TestInsertionSort()
        {
            service.IntArrayInsertionSort(data);

            Assert.IsNotNull(data);
        }

        [Ignore]
        [TestMethod]
        public void TestShellSortNaive()
        {
            service.IntArrayShellSortNaive(data);

            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TestBuildHeap()
        {
            int[] arr = { 1, 3, 5, 4, 6, 13, 10,
                    9, 8, 15, 17 };

            int n = arr.Length;

            BuildHeap.buildHeap(arr, n);

            BuildHeap.printHeap(arr, n);

            Assert.IsNotNull(arr);
        }

        //[TestMethod]
        //public void QuadtraticEquation()
        //{
        //    var quadFormula = "b^2+-SquareRoot(b^2-4ac) / 2a";
        //    var problem = "2x^2 + 8x - 6 = 0";
        //    // divide by 2
        //    problem = "x^2 + 4x = 3";
        //    // chop in 4x two
        //    problem = @"[2x][2^2]
        //                [x^2][2x] = 3 +[2^2]"; // ==7
        //    problem = "(x+2)^2 = 7";
        //    problem = "x = -2 + SquareRoot(7)";
        //    // keep equality

        //}

        /// <summary>
        /// https://www.youtube.com/watch?v=N-KXStupwsc
        /// nicolo' tartaglia 1500 era
        /// scipione del ferro
        /// gerolamo cardano
        /// </summary>
        //[TestMethod]
        //public void SecretCubicFormula()
        //{
        //    var formula = "ax^3+bx^2+cx+d=0";
        //    // set a = 1;
        //    // set b = 0;
        //    // change c/d = p/q
        //    var newFormula = "x^3 +px +q =0";


        //}

        [TestMethod]
        public void TestLog()
        {
            var value = Math.Log(81, 3);
            var f = Math.Log(data.Length, 3) - 1;
            //var g = Math.Max(1, f);
            Assert.IsNotNull(value);
            Assert.IsNotNull(f);
        }
        [TestMethod]
        public void TestShellSortBetterSort()
        {
            service.IntArrayShellSortBetter(data);

            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void ReverseArray()
        {
            int[] array = { 1, 4, 3, 2 };
            service.ReverseArray(array);

            Assert.IsTrue(true);
        }
    }
}
