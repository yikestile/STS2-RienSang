namespace RienSang.RienSangCode.Extensions;

public static class StringExtensions
{
    public static string ImagePath(this string path)
    {
        return $"res://{MainFile.ModId}/images/{path}";
    }

    public static string CardImagePath(this string path)
    {
        return $"res://{MainFile.ModId}/images/card_portraits/big/{path}";
    }

    public static string BigCardImagePath(this string path)
    {
        return $"res://{MainFile.ModId}/images/card_portraits/big/{path}";
    }

    public static string PowerImagePath(this string path)
    {
        return $"res://{MainFile.ModId}/images/powers/{path}";
    }

    public static string BigPowerImagePath(this string path)
    {
        return $"res://{MainFile.ModId}/images/powers/{path}";
    }

    public static string RelicImagePath(this string path)
    {
        return $"res://{MainFile.ModId}/images/relics/big/{path}";
    }
    
    public static string RelicOutlineImagePath(this string path)
    {
        return $"res://{MainFile.ModId}/images/relics/big/{path}";
    }

    public static string BigRelicImagePath(this string path)
    {
        return $"res://{MainFile.ModId}/images/relics/big/{path}";
    }

    public static string CharacterUiPath(this string path)
    {
        return $"res://{MainFile.ModId}/images/riensang/{path}";
    }
    
    public static string PlaceholderImagePath(this string path)
    {
        return $"res://{MainFile.ModId}/images/placeholder/{path}";
    }
}