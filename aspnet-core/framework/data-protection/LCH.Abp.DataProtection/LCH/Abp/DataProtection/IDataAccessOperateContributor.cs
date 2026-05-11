using System.Linq.Expressions;

namespace LCH.Abp.DataProtection;
public interface IDataAccessOperateContributor
{
    DataAccessFilterOperate Operate { get; }
    Expression BuildExpression(Expression left, Expression right);
}
