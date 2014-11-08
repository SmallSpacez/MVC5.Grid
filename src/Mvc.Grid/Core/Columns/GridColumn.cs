﻿using System;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace NonFactors.Mvc.Grid
{
    public class GridColumn<TModel, TValue> : BaseGridColumn, IGridColumn<TModel, TValue> where TModel : class
    {
        public Expression<Func<TModel, TValue>> Expression { get; set; }
        public GridProcessorType Type { get; set; }
        public IGrid<TModel> Grid { get; set; }

        public GridColumn(IGrid<TModel> grid, Expression<Func<TModel, TValue>> expression)
        {
            Grid = grid;
            IsEncoded = true;
            Expression = expression;
            Type = GridProcessorType.Pre;
            Name = ExpressionHelper.GetExpressionText(expression);
            SortOrder = Grid.Query.GetSortingQuery(Name).SortOrder;
        }

        public IQueryable<TModel> Process(IQueryable<TModel> items)
        {
            if (IsSortable != true)
                return items;

            if (SortOrder == null)
                return items;

            if (SortOrder == GridSortOrder.Asc)
                return items.OrderBy(Expression);

            return items.OrderByDescending(Expression);
        }

        public override IHtmlString ValueFor(IGridRow row)
        {
            String value = GetRawValueFor(row);
            if (IsEncoded) value = WebUtility.HtmlEncode(value);

            return new HtmlString(value);
        }

        private String GetRawValueFor(IGridRow row)
        {
            TValue value = Expression.Compile()(row.Model as TModel);
            if (value == null)
                return String.Empty;

            if (Format == null)
                return value.ToString();

            return String.Format(Format, value);
        }

        public override String LinkForSort()
        {
            if (!(IsSortable == true))
                return "#";

            String sortKey = "MG-Sort-" + Grid.Name;
            Regex gridSort = new Regex(sortKey + "-" + "[^-]*$");
            NameValueCollection query = new NameValueCollection(Grid.Query.Query);
            foreach (String key in query.AllKeys.Where(key => gridSort.IsMatch(key)))
                query.Remove(key);

            if (SortOrder == GridSortOrder.Asc)
                query[sortKey + "-" + Name] = GridSortOrder.Desc.ToString();
            else
                query[sortKey + "-" + Name] = GridSortOrder.Asc.ToString();

            return "?" + String.Join("&", query.AllKeys.Select(key => HttpUtility.UrlPathEncode(key + "=" + query[key])));
        }
    }
}
