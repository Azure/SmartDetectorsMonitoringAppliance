export default class FormatUtils {
    static kmNumber(num: any, postfix?: string): string {
        if (isNaN(num)) { return num + (postfix || ''); }
    
        let value = parseFloat(num);
    
        return (
          value > 999999999 ?
          (value / 1000000000).toFixed(1) + 'B' :
            value > 999999 ?
              (value / 1000000).toFixed(1) + 'M' :
              value > 999 ?
                (value / 1000).toFixed(1) + 'K' : 
                  (value % 1 * 10) !== 0 ?
                  value.toFixed(1).toString() : value.toString()) + (postfix || '');
        }
}