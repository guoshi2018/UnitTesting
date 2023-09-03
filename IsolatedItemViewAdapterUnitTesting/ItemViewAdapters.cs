
using Android.Widget;
using Android.Views;
using System.Collections.Generic;
using System;
using Android.Util;
using Android.Content;
using Android.App;
using System.Runtime.Remoting.Contexts;
//using System.Runtime.Remoting.Contexts;
using Android.Runtime;
using Java.Util.Zip;

#nullable enable



namespace IsolatedItemViewAdapterUnitTesting
{

    public class CustomArrayAdapter<TData> : ArrayAdapter<TData>
    {
        protected readonly int _itemViewLayoutId;
        protected readonly Action<View, int, ViewGroup?>? _renderItemView;
        protected readonly LayoutInflater _inflater;

        public CustomArrayAdapter(Android.Content.Context context, int itemViewLayoutId, TData[] datas,
            Action<View, int, ViewGroup?>? renderItemView = null)
            : base(context, itemViewLayoutId, datas)
        {
            _renderItemView = renderItemView;
            var inflater = context.GetSystemService(Android.Content.Context.LayoutInflaterService).JavaCast<LayoutInflater>();
            if (null == inflater)
                throw new AndroidException("failed to get LayoutInflaterService");
            _inflater = inflater;

            this.SignDebug("1");
        }

        // tuple as params
        public CustomArrayAdapter((Android.Content.Context context, int itemViewLayoutId, TData[] datas,
            Action<View, int, ViewGroup?>? renderItemView) args) : this(args.context, args.itemViewLayoutId, args.datas, args.renderItemView) { }

        public CustomArrayAdapter((Android.Content.Context context, int itemViewLayoutId, TData[] datas) args)
            : this(args.context, args.itemViewLayoutId, args.datas, null) { }

        //public override View GetView(int position, View? convertView, ViewGroup parent)
        //{
        //    //View? view = convertView; // re-use an existing view, if one is available
        //    //if (view == null)// otherwise create a new one
        //    //{
        //    //    this.SignDebug("2");
        //    //    //    view = this.Context.LayoutInflater.Inflate(_itemViewLayoutId, parent, false);
        //    //    view = _inflater.Inflate(_itemViewLayoutId, null);
        //    //    this.SignDebug("3");
        //    //    if (view == null)
        //    //        throw new AndroidRuntimeException("At least one shared view creation failed");
        //    //    _renderItemView?.Invoke(view, position, null);
        //    //    this.SignDebug("4");
        //    //}
        //    this.SignDebug("2");

        //    View view;
        //    try
        //    {
        //        view = base.GetView(position, convertView, parent);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.SignDebug(ex.Message);
        //        throw;
        //    }

        //    this.SignDebug("3");
        //    //  _renderItemView?.Invoke(view, position, null);
        //    return view;
        //}
    }





    // note： addView(View, LayoutParams) is not supported in AdapterView

    /// <summary>
    /// The item views are isolated
    /// </summary>
    public class IsolatedItemViewAdapter<TData> : BaseItemViewAdapter<TData>
    {
        private readonly Action<View, int, ViewGroup?>? _reRenderItemView;

        public IsolatedItemViewAdapter(Android.Content.Context context, int itemViewLayoutId, TData[] datas,
            Action<View, int, ViewGroup?>? renderItemView = null, Action<View, int, ViewGroup?>? reRenderItemView = null)
            : base(context, itemViewLayoutId, datas, renderItemView) => _reRenderItemView = reRenderItemView;

        // tuple as params
        public IsolatedItemViewAdapter((Android.Content.Context context, int itemViewLayoutId, TData[] datas,
            Action<View, int, ViewGroup?>? renderItemView, Action<View, int, ViewGroup?>? reRenderItemView) args)
        : this(args.context, args.itemViewLayoutId, args.datas, args.renderItemView, args.reRenderItemView) { }

        public IsolatedItemViewAdapter((Android.Content.Context context, int itemViewLayoutId, TData[] datas,
            Action<View, int, ViewGroup?>? renderItemView) args)
        : this(args.context, args.itemViewLayoutId, args.datas, args.renderItemView, null) { }

        public IsolatedItemViewAdapter((Android.Content.Context context, int itemViewLayoutId, TData[] datas) args)
        : this(args.context, args.itemViewLayoutId, args.datas, null, null) { }


        protected override void _InitializeWrappers(TData[] datas)
        {
            for (var i = 0; i < datas.Length; i++)
            {
                //View? v = _context.LayoutInflater.Inflate(_itemViewLayoutId, null)
                //    ?? throw new AndroidRuntimeException("At least one isolated view creation failed");
                View? v = _inflater.Inflate(_itemViewLayoutId, null)
                    ?? throw new AndroidRuntimeException("At least one isolated view creation failed");
                _wrappers.Add(new ViewInfoWrapper(i, datas[i], v, false));
            }
        }

        public override View GetView(int position, View? convertView, ViewGroup? parent)
        {
            // _wrappers[position].AssView is guaranteed not to be null
            View v = _wrappers[position].AssView!;

            if (!_wrappers[position].Ready)
            {
                if (_renderItemView != null)
                    _renderItemView(v, position, parent);
                _wrappers[position].Ready = true;
                //    Log.Debug("first GetView in isolated", $"when <{position}> creation,parent:{v.Parent == parent}");
            }
            else
            {
                //       Log.Debug("other GetView in isolated", $"when <{position}> creation, parent :{v.Parent == parent}");
                _reRenderItemView?.Invoke(v, position, parent);
            }
            return v;
        }
    }

    /// <summary>
    /// The item views are shared
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class SharedItemViewAdapter<TData> : BaseItemViewAdapter<TData>
    {
        public SharedItemViewAdapter(Android.Content.Context context, int itemViewLayoutId, TData[] datas,
            Action<View, int, ViewGroup?>? renderItemView = null)
            : base(context, itemViewLayoutId, datas, renderItemView) { }

        // tuple as params
        public SharedItemViewAdapter((Android.Content.Context context, int itemViewLayoutId, TData[] datas,
            Action<View, int, ViewGroup?>? renderItemView) args) : this(args.context, args.itemViewLayoutId, args.datas, args.renderItemView) { }

        public SharedItemViewAdapter((Android.Content.Context context, int itemViewLayoutId, TData[] datas) args)
            : this(args.context, args.itemViewLayoutId, args.datas, null) { }

        protected override void _InitializeWrappers(TData[] datas)
        {
            for (var i = 0; i < datas.Length; i++)
                _wrappers.Add(new ViewInfoWrapper(i, datas[i], null, false));
        }

        public override View GetView(int position, View? convertView, ViewGroup? parent)
        {
            View? view = convertView; // re-use an existing view, if one is available
            if (view == null)// otherwise create a new one
            {
                //    view = _context.LayoutInflater.Inflate(_itemViewLayoutId, parent, false);
                view = _inflater.Inflate(_itemViewLayoutId, parent, false);
                if (view == null)
                    throw new AndroidRuntimeException("At least one shared view creation failed");
            }

            Log.Debug("GetView in shared", $"when <{position}> creation, parent :{view.Parent}");
            _wrappers[position].AssView = view;
            _wrappers[position].Ready = true;
            _renderItemView?.Invoke(view, position, parent);

            return view;
        }
    }
    public abstract class BaseItemViewAdapter<TData> : BaseAdapter<TData>
    {
        protected readonly List<ViewInfoWrapper> _wrappers;
        protected readonly Android.Content.Context _context;
        protected readonly int _itemViewLayoutId;
        protected readonly Action<View, int, ViewGroup?>? _renderItemView;
        protected readonly LayoutInflater _inflater;

        protected BaseItemViewAdapter(Android.Content.Context context, int itemViewLayoutId, TData[] datas,
            Action<View, int, ViewGroup?>? renderItemView = null)
            : base()
        {
            (_context, _itemViewLayoutId, _renderItemView) = (context, itemViewLayoutId, renderItemView);
            _wrappers = new List<ViewInfoWrapper>();
            var inflater = context.GetSystemService(Android.Content.Context.LayoutInflaterService).JavaCast<LayoutInflater>();
            if (null == inflater)
                throw new AndroidException("failed to get LayoutInflaterService");
            _inflater = inflater;
            _InitializeWrappers(datas);
        }

        // tuple as params
        protected BaseItemViewAdapter((Android.Content.Context context, int itemViewLayoutId, TData[] datas,
            Action<View, int, ViewGroup?>? renderItemView) args) : this(args.context, args.itemViewLayoutId, args.datas, args.renderItemView) { }


        protected abstract void _InitializeWrappers(TData[] datas);

        public override long GetItemId(int position)
        {
            return position;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return _wrappers[position];
        }
        public override TData this[int position]
        {
            get { return _wrappers[position].Data; }
        }
        public override int Count
        {
            get { return _wrappers.Count; }
        }
        protected class ViewInfoWrapper : Java.Lang.Object
        {
            public int Position { get; private set; }

            public TData Data { get; private set; }

            public View? AssView { get; set; }

            public bool Ready { get; set; }

            public ViewInfoWrapper(int position, TData data, View? assView = null, bool ready = false) =>
            (Position, Data, AssView, Ready) = (position, data, assView, ready);
        }
    }

}