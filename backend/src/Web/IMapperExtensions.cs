using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Web.Exceptions;

namespace Web
{
    public static class IMapperExtensions
    {
        public static TDest MapDto<TSource, TDest>(this IMapper mapper, TSource src)
        {
            try
            {
                return mapper.Map<TSource, TDest>(src);
            }
            catch (Exception e)
            {
                throw new ApiException(HttpStatusCode.BadRequest, "Invalid request", e);
            }
        }
    }
}
