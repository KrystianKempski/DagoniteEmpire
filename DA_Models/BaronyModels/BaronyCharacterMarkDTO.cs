using DA_Common.Barony;

namespace DA_Models.BaronyModels
{
    public class BaronyCharacterMarkDTO
    {
        public string? IconKey { get; set; }
        public string? ColorKey { get; set; }

        public bool IsSet => BaronyCharacterMarkCatalog.IsSet(IconKey, ColorKey);

        public BaronyCharacterMark? ToMark() => BaronyCharacterMarkCatalog.Normalize(IconKey, ColorKey);

        public static BaronyCharacterMarkDTO? FromKeys(string? iconKey, string? colorKey)
        {
            var mark = BaronyCharacterMarkCatalog.Normalize(iconKey, colorKey);
            if (mark is null)
                return null;

            return new BaronyCharacterMarkDTO
            {
                IconKey = mark.Value.IconKey,
                ColorKey = mark.Value.ColorKey,
            };
        }

        public static BaronyCharacterMarkDTO? FromMark(BaronyCharacterMark? mark) =>
            mark is null || !mark.Value.IsSet
                ? null
                : new BaronyCharacterMarkDTO
                {
                    IconKey = mark.Value.IconKey,
                    ColorKey = mark.Value.ColorKey,
                };

        public static List<BaronyCharacterMarkDTO> NormalizeList(IEnumerable<BaronyCharacterMarkDTO>? marks)
        {
            var list = new List<BaronyCharacterMarkDTO>();
            if (marks is null)
                return list;

            foreach (var mark in marks)
            {
                var normalized = FromKeys(mark.IconKey, mark.ColorKey);
                if (normalized is null)
                    continue;

                if (list.Any(existing =>
                        existing.ToMark() is BaronyCharacterMark m
                        && normalized.ToMark() is BaronyCharacterMark n
                        && BaronyCharacterMarkCatalog.Same(m, n)))
                    continue;

                list.Add(normalized);
            }

            return list;
        }

        public static List<BaronyCharacterMarkDTO> Toggle(
            IReadOnlyList<BaronyCharacterMarkDTO> marks,
            string? iconKey,
            string? colorKey)
        {
            var next = NormalizeList(marks);
            var mark = BaronyCharacterMarkCatalog.Normalize(iconKey, colorKey);
            if (mark is null)
                return next;

            var existing = next.FindIndex(m =>
                m.ToMark() is BaronyCharacterMark existingMark
                && BaronyCharacterMarkCatalog.Same(existingMark, mark.Value));

            if (existing >= 0)
                next.RemoveAt(existing);
            else
                next.Add(FromMark(mark)!);

            return next;
        }
    }
}
