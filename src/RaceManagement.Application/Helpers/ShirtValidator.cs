using RaceManagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Application.Helpers
{
    public static class ShirtValidator
    {
        /// <summary>
        /// Kiểm tra đăng ký áo có hợp lệ so với cấu hình RaceShirtTypes
        /// </summary>
        public static bool ValidateShirtSelection(Registration registration, Race race, out string error)
        {
            error = string.Empty;

            // Nếu race không có bán áo thì cho qua
            if (!race.HasShirtSale)
                return true;

            // Nếu thiếu thông tin
            if (string.IsNullOrWhiteSpace(registration.ShirtCategory) ||
                string.IsNullOrWhiteSpace(registration.ShirtType) ||
                string.IsNullOrWhiteSpace(registration.ShirtSize))
            {
                error = "Thiếu thông tin áo (Category / Type / Size)";
                return false;
            }

            // Tìm loại áo phù hợp
            var matchingType = race.ShirtTypes.FirstOrDefault(st =>
                st.IsActive &&
                st.ShirtCategory.Equals(registration.ShirtCategory, StringComparison.OrdinalIgnoreCase) &&
                st.ShirtType.Equals(registration.ShirtType, StringComparison.OrdinalIgnoreCase));

            if (matchingType == null)
            {
                error = $"Loại áo '{registration.ShirtCategory} - {registration.ShirtType}' không hợp lệ";
                return false;
            }

            // Kiểm tra size hợp lệ
            var validSizes = matchingType.AvailableSizes
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToUpper())
                .ToList();

            if (!validSizes.Contains(registration.ShirtSize.Trim().ToUpper()))
            {
                error = $"Size '{registration.ShirtSize}' không hợp lệ cho {matchingType.GetDisplayName}. " +
                        $"Các size hợp lệ: {string.Join(", ", validSizes)}";
                return false;
            }

            return true;
        }
    }
}
