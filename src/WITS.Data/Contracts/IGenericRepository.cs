// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  � 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

using WITS.Common;
using WITS.Data.Common;

namespace WITS.Data.Contracts;

public interface IGenericRepository<T, TDataType> where T : BaseEntity<TDataType>
{
    Task<PagedResult<T>> GetAllAsync(int pageNumber = 1, int pageSize = 100);
    Task<PagedResult<T>> QueryAsync(GenericFilter filter);
    Task<T?> GetByIdAsync(TDataType id);
    Task<int> InsertAsync(T model);
    Task<bool> UpdateAsync(T model);
    Task<bool> DeleteByIdAsync(TDataType id);
    Task<PagedResult<T>> SearchAsync(SearchParams searchParams);
}
