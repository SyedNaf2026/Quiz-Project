import { HttpInterceptorFn } from '@angular/common/http';
import { map } from 'rxjs/operators';

// Recursively converts all object keys from PascalCase/any-case to camelCase
function toCamel(obj: any): any {
  if (Array.isArray(obj)) {
    return obj.map(toCamel);
  }
  if (obj !== null && typeof obj === 'object') {
    const result: any = {};
    for (const key of Object.keys(obj)) {
      const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
      result[camelKey] = toCamel(obj[key]);
    }
    return result;
  }
  return obj;
}

export const camelCaseInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    map((event: any) => {
      if (event?.body !== undefined) {
        return event.clone({ body: toCamel(event.body) });
      }
      return event;
    })
  );
};
