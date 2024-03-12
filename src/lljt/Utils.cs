using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lljt;

internal class Utils {
    public static string? ExtractFieldValueFromLine(string? line, string field) {
        if (line is null) {
            return null;
        }

        var parts = line?.Remove(0, 2).Split(": ");

        if (parts[0] != field) {
            return null;
        }

        return parts[1];
    }
}
