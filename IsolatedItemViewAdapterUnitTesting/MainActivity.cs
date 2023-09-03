using System;
using Android.App;
using Android.OS;
using AndroidX.AppCompat.App;
using Android.Widget;
using System.Linq;
using Android.Views;
using Android.Util;

#nullable enable

namespace IsolatedItemViewAdapterUnitTesting
{
    // 1. If item count is less than or equal to the threshold(e.g. 5), which depends on how many a screen can hold, the program is  OK;
    // otherwize, all event handlers lost, except for the last few at the bottom!
    // 2. It is no happy with ListView or with GridView when amount is greater than the threshold
    // and there are more errors if we use SharedItemViewAdapter(the row view shared form).
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        #region Switch between various ways
        private ContainerWays _contWay;
        private AmountWays _amtWay;
        private AdapterWays _adpWay;
        #endregion

        private const string TEXT_ENABLE = "Enable";
        private const string TEXT_DISABLE = "Disable";

        private ListView? _lv_container;
        private GridView? _gv_container;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.main);

            _lv_container = FindViewById<ListView>(Resource.Id.lv_container)!;
            _gv_container = FindViewById<GridView>(Resource.Id.gv_container)!;

            __fillSpinner<ContainerWays>(Resource.Id.spn_container, newWay => _contWay = newWay);
            __fillSpinner<AmountWays>(Resource.Id.spn_amount, newWay => _amtWay = newWay);
            __fillSpinner<AdapterWays>(Resource.Id.spn_adapter, newWay => _adpWay = newWay);
        }

        private void __fillSpinner<TEnum>(int spinnerResId, Action<TEnum> callback) where TEnum : notnull, Enum
        {
            var spn = FindViewById<Spinner>(spinnerResId);
            if (spn != null)
            {
                TEnum[]? ways = Enum.GetValues(typeof(TEnum)) as TEnum[];
                spn.Adapter = new ArrayAdapter<TEnum>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, ways!);
                spn.ItemSelected += (sender, e) =>
                {
                    var str = (sender as Spinner)!.SelectedItem?.ToString();
                    object v = Enum.Parse(typeof(TEnum), str);
                    callback((TEnum)v);
                    __UpdateViews();
                };
            }
        }

        private void __UpdateViews()
        {
            Log.Debug("style", $"{_amtWay}-{_contWay}-{_adpWay}");
            __ClearAll();
            // how many item views 
            int amount = (int)_amtWay;
            HelloUIButler[] helloButlers = Enumerable.Range(1, amount).Select(_ => new HelloUIButler()).ToArray();
            (Activity context, int itemViewLayoutId, HelloUIButler[] datas, Action<View, int, ViewGroup?>? renderItemView) args =
                (this, Resource.Layout.item, helloButlers,
                (view, position, parent) =>
                {
                    Button? btnEnable = view!.FindViewById<Button>(Resource.Id.btn_enable);
                    Button? btnCall = view.FindViewById<Button>(Resource.Id.btn_call);
                    EditText? etResult = view.FindViewById<EditText>(Resource.Id.et_result);
                    etResult!.Text = "";

                    var helloBt = helloButlers[position];
                    helloBt.BtnEnable = btnEnable;
                    helloBt.BtnCall = btnCall;
                    helloBt.TextEnable = $"{TEXT_ENABLE} {position + 1}";
                    helloBt.TextDisable = $"{TEXT_DISABLE} {position + 1}";
                    helloBt.Callback = () =>
                    {
                        etResult?.Append($"hello,{DateTime.Now.ToLongTimeString()}{System.Environment.NewLine}");
                    };
                    helloBt.Work();
                }
            );

            // what kind of parent view: ListView or GridView
            AbsListView? container = _contWay switch
            {
                ContainerWays.ListView => _lv_container,
                _ => _gv_container,
            };

            // what kind of adapter: Isolated or Shared
            IListAdapter adapter = _adpWay switch
            {
                AdapterWays.Isolated => new IsolatedItemViewAdapter<HelloUIButler>(args),
                AdapterWays.Shared => new SharedItemViewAdapter<HelloUIButler>(args),
                _ => new CustomArrayAdapter<HelloUIButler>(args),
            };

            //     this.SignDebug("three");
            // run
            if (container != null)
                container.Adapter = adapter;
        }

        private void __ClearAll()
        {
            __ClearContainer(_lv_container);
            __ClearContainer(_gv_container);
        }

        //private void __ClearContainer(AbsListView? container)
        //{
        //    if (container != null && container.ChildCount > 0)
        //    {
        //        this.SignDebug("one");
        //        try
        //        {
        //            container.RemoveAllViews(); // exception: removeAllViews() is not supported in AdapterView
        //            this.SignDebug("two");
        //        }
        //        catch (Exception ex)
        //        {
        //            this.SignDebug(ex.Message);
        //        }
        //        container.Adapter = null;
        //    }
        //    this.SignDebug("three");
        //}
        private void __ClearContainer(AbsListView? container)
        {
            if (container != null && container.ChildCount > 0 && container.Adapter != null)
            {
                this.SignDebug($"one for {container.AccessibilityClassName}");
                container.Adapter = null;
            }
            this.SignDebug($"two for {container?.AccessibilityClassName}");
        }


        private enum ContainerWays
        {
            ListView,
            GridView,
        }
        private enum AmountWays
        {
            _1 = 1,
            _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13, _14, _15, _16, _17, _18, _19, _20, _21, _22, _23, _24, _25, _26, _27, _28, _29, _30
        }
        private enum AdapterWays
        {
            CustomArray,
            Isolated,
            Shared,

        }
    }

    public static class TempExtension
    {
        public static void SignDebug(this Object obj, string flag)
        {
            System.Diagnostics.Debug.WriteLine($"..........................{flag}............................");
        }
    }
}

/*
 Title:
    Java.Lang.Object in Custom ItemViewAdapter for Android lost event handlers when item count is a little big

I  created two Adapter classes inheriting from BaseAdapter, which attempt to eliminate some unnecessary coupling:

    1. SharedItemViewAdapter<TData>:  GetView method uses classic form to share views across multiple items;

    2. IsolatedItemViewAdapter<TData>: GetView method is in a radical form, an attempt is made to hold event handlers, if required, for child controls within individual row views. That is, an item corresponds to a unique line view，although there may be significant memory overhead.

Question：

    1. If item count is less than or equal to the threshold(e.g. 5), which depends on how many a screen can hold, the program is  OK; otherwize, all event handlers lost, except for the last few at the bottom!

    2. It is no happy with ListView or with GridView when amount is greater than the threshold and there are more errors if we use SharedItemViewAdapter(the row view shared form).

I wonder whether it's garbage collection error action, and how to solve it?

This problem has been bothering me for a long time, and I beg all the gods for help!

Project Source code: https://github.com/guoshi2018/UnitTesting/tree/master/IsolatedItemViewAdapterUnitTesting

If github is difficult to open, this is Baidu disk: 
https://pan.baidu.com/s/1JRgyTaFy4rj1hZlMmEYakw?pwd=s96e 
提取码：s96e

Thanks a lot !
 * */

