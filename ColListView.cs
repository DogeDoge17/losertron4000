using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace losertron4000
{
    public class ColListView<T> where T : ImageItem, new()
    {
        private ListView _view;

        public ObservableCollection<ObservableCollection<T>> observableLists;


        public ColListView(ObservableCollection<T> list, ListView view, int columns)
        {
            _view = view;

            observableLists = new ObservableCollection<ObservableCollection<T>>();
            

            int switc = 0;
            for (int i = 0; i < list.Count; i+= columns)
            {
                observableLists.Add(new ObservableCollection<T>());

                for(int j = 0; j < columns; j++)
                {
                    if (j + i >= list.Count)
                        observableLists[observableLists.Count - 1].Add(new T());
                    else
                        observableLists[observableLists.Count - 1].Add(list[i +j]);
                }

                //if (switc >= observableLists.Count)
                //    switc = 0;

                //observableLists[switc].Add(list[i]);
                //switc++;
            }

            //for (int i = 1; i < observableLists.Count; i++)
            //{
            //    int addTarg = observableLists[0].Count - observableLists[i].Count;
            //    for (int j = 0; j < addTarg; j++)
            //    {
            //        observableLists[i].Add(default(T));
            //    }
            //}

            view.ItemsSource = observableLists;
        }

        public T this[int index]
        {
            get
            {
                return default(T);
            }
            set
            {

            }
        }


        public List<T> FullList
        {
            get
            {
                int fullCount = 0;
                for (int i = 0; i < observableLists.Count; i++)
                {
                    fullCount += observableLists[i].Count;
                }

                List<T> mega = new List<T>(fullCount);

                for (int i = 0; i < observableLists.Count; i++)
                {
                    for (int j = 0; j < observableLists[i].Count; j += observableLists.Count)
                    {
                        mega.Add(observableLists[i][j]);
                    }
                }

                return mega;
            }
        }
    }
    //public class ColListView<T>
    //{
    //    private List<ListView> _views = new();

    //    public ObservableCollection<T>[] observableLists;


    //    public ColListView(ObservableCollection<T> list, params List<ListView> views)
    //    {
    //        _views = views;

    //        int switc = 0;
    //        observableLists = new ObservableCollection<T>[views.Count];

    //        for (int i = 0; i < observableLists.Length; i++)
    //        {
    //            observableLists[i] = new();
    //        }

    //        for (int i = 0; i < list.Count; i++)
    //        {
    //            if (switc >= observableLists.Length)
    //                switc = 0;

    //            observableLists[switc].Add(list[i]);
    //            switc++;
    //        }

    //        for (int i = 0; i < observableLists.Length; i++)
    //        {
    //            views[i].ItemsSource = observableLists[i];
    //        }
    //    }

    //    public T this[int index]
    //    {
    //        get
    //        {
    //            return default(T);
    //        }
    //        set
    //        {

    //        }
    //    }


    //    public List<T> FullList
    //    {
    //        get
    //        {
    //            int fullCount = 0;
    //            for (int i = 0; i < observableLists.Length; i++)
    //            {
    //                fullCount += observableLists[i].Count;
    //            }

    //            List<T> mega = new List<T>(fullCount);

    //            for (int i = 0; i < observableLists.Length; i++)
    //            {
    //                for (int j = 0; j < observableLists[i].Count; j += observableLists.Length)
    //                {
    //                    mega.Add(observableLists[i][j]);
    //                }
    //            }

    //            return mega;
    //        }
    //    }
    //}
}
