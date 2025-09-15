namespace Payment.Domain.ValueObjects;

/// <summary>
/// Value Object représentant une devise
/// </summary>
public class Currency
{
    /// <summary>
    /// Code ISO 4217 de la devise
    /// </summary>
    public string Code { get; private set; }

    /// <summary>
    /// Nom de la devise
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Symbole de la devise
    /// </summary>
    public string Symbol { get; private set; }

    /// <summary>
    /// Nombre de décimales
    /// </summary>
    public int DecimalPlaces { get; private set; }

    /// <summary>
    /// Constructeur pour Entity Framework
    /// </summary>
    private Currency() 
    {
        Code = string.Empty;
        Name = string.Empty;
        Symbol = string.Empty;
    }

    /// <summary>
    /// Constructeur principal
    /// </summary>
    public Currency(string code, string name, string symbol, int decimalPlaces = 2)
    {
        Code = code?.ToUpper() ?? throw new ArgumentNullException(nameof(code));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
        DecimalPlaces = decimalPlaces;

        ValidateCurrency();
    }

    /// <summary>
    /// Créer une devise à partir du code ISO
    /// </summary>
    public static Currency FromCode(string code)
    {
        var upperCode = code?.ToUpper() ?? throw new ArgumentNullException(nameof(code));

        return upperCode switch
        {
            "EUR" => new Currency("EUR", "Euro", "€", 2),
            "USD" => new Currency("USD", "US Dollar", "$", 2),
            "GBP" => new Currency("GBP", "British Pound", "£", 2),
            "CHF" => new Currency("CHF", "Swiss Franc", "CHF", 2),
            "JPY" => new Currency("JPY", "Japanese Yen", "¥", 0),
            "CAD" => new Currency("CAD", "Canadian Dollar", "C$", 2),
            "AUD" => new Currency("AUD", "Australian Dollar", "A$", 2),
            "SEK" => new Currency("SEK", "Swedish Krona", "kr", 2),
            "NOK" => new Currency("NOK", "Norwegian Krone", "kr", 2),
            "DKK" => new Currency("DKK", "Danish Krone", "kr", 2),
            "PLN" => new Currency("PLN", "Polish Zloty", "zł", 2),
            "CZK" => new Currency("CZK", "Czech Koruna", "Kč", 2),
            "HUF" => new Currency("HUF", "Hungarian Forint", "Ft", 2),
            "BGN" => new Currency("BGN", "Bulgarian Lev", "лв", 2),
            "RON" => new Currency("RON", "Romanian Leu", "lei", 2),
            "HRK" => new Currency("HRK", "Croatian Kuna", "kn", 2),
            "RUB" => new Currency("RUB", "Russian Ruble", "₽", 2),
            "TRY" => new Currency("TRY", "Turkish Lira", "₺", 2),
            "BRL" => new Currency("BRL", "Brazilian Real", "R$", 2),
            "CNY" => new Currency("CNY", "Chinese Yuan", "¥", 2),
            "INR" => new Currency("INR", "Indian Rupee", "₹", 2),
            "KRW" => new Currency("KRW", "South Korean Won", "₩", 0),
            "SGD" => new Currency("SGD", "Singapore Dollar", "S$", 2),
            "HKD" => new Currency("HKD", "Hong Kong Dollar", "HK$", 2),
            "MXN" => new Currency("MXN", "Mexican Peso", "$", 2),
            "ZAR" => new Currency("ZAR", "South African Rand", "R", 2),
            "NZD" => new Currency("NZD", "New Zealand Dollar", "NZ$", 2),
            "ILS" => new Currency("ILS", "Israeli Shekel", "₪", 2),
            "AED" => new Currency("AED", "UAE Dirham", "د.إ", 2),
            "SAR" => new Currency("SAR", "Saudi Riyal", "﷼", 2),
            "EGP" => new Currency("EGP", "Egyptian Pound", "£", 2),
            "QAR" => new Currency("QAR", "Qatari Riyal", "﷼", 2),
            "KWD" => new Currency("KWD", "Kuwaiti Dinar", "د.ك", 3),
            "BHD" => new Currency("BHD", "Bahraini Dinar", ".د.ب", 3),
            "OMR" => new Currency("OMR", "Omani Rial", "﷼", 3),
            "JOD" => new Currency("JOD", "Jordanian Dinar", "د.ا", 3),
            "LBP" => new Currency("LBP", "Lebanese Pound", "ل.ل", 2),
            "MAD" => new Currency("MAD", "Moroccan Dirham", "د.م.", 2),
            "TND" => new Currency("TND", "Tunisian Dinar", "د.ت", 3),
            "DZD" => new Currency("DZD", "Algerian Dinar", "د.ج", 2),
            "XAF" => new Currency("XAF", "Central African CFA Franc", "FCFA", 0),
            "XOF" => new Currency("XOF", "West African CFA Franc", "CFA", 0),
            "NGN" => new Currency("NGN", "Nigerian Naira", "₦", 2),
            "GHS" => new Currency("GHS", "Ghanaian Cedi", "₵", 2),
            "KES" => new Currency("KES", "Kenyan Shilling", "KSh", 2),
            "UGX" => new Currency("UGX", "Ugandan Shilling", "USh", 0),
            "TZS" => new Currency("TZS", "Tanzanian Shilling", "TSh", 2),
            "ETB" => new Currency("ETB", "Ethiopian Birr", "Br", 2),
            "XBT" => new Currency("XBT", "Bitcoin", "₿", 8),
            "ETH" => new Currency("ETH", "Ethereum", "Ξ", 18),
            "LTC" => new Currency("LTC", "Litecoin", "Ł", 8),
            "BCH" => new Currency("BCH", "Bitcoin Cash", "₿", 8),
            "XRP" => new Currency("XRP", "Ripple", "XRP", 6),
            "ADA" => new Currency("ADA", "Cardano", "₳", 6),
            "DOT" => new Currency("DOT", "Polkadot", "DOT", 10),
            "LINK" => new Currency("LINK", "Chainlink", "LINK", 18),
            "UNI" => new Currency("UNI", "Uniswap", "UNI", 18),
            "MATIC" => new Currency("MATIC", "Polygon", "MATIC", 18),
            "SOL" => new Currency("SOL", "Solana", "SOL", 9),
            "AVAX" => new Currency("AVAX", "Avalanche", "AVAX", 18),
            "FTM" => new Currency("FTM", "Fantom", "FTM", 18),
            "ALGO" => new Currency("ALGO", "Algorand", "ALGO", 6),
            "ATOM" => new Currency("ATOM", "Cosmos", "ATOM", 6),
            "NEAR" => new Currency("NEAR", "NEAR Protocol", "NEAR", 24),
            "FLOW" => new Currency("FLOW", "Flow", "FLOW", 8),
            "ICP" => new Currency("ICP", "Internet Computer", "ICP", 8),
            "FIL" => new Currency("FIL", "Filecoin", "FIL", 18),
            "VET" => new Currency("VET", "VeChain", "VET", 18),
            "TRX" => new Currency("TRX", "TRON", "TRX", 6),
            "EOS" => new Currency("EOS", "EOS", "EOS", 4),
            "XTZ" => new Currency("XTZ", "Tezos", "XTZ", 6),
            "THETA" => new Currency("THETA", "Theta", "THETA", 18),
            "AAVE" => new Currency("AAVE", "Aave", "AAVE", 18),
            "MKR" => new Currency("MKR", "Maker", "MKR", 18),
            "COMP" => new Currency("COMP", "Compound", "COMP", 18),
            "SUSHI" => new Currency("SUSHI", "SushiSwap", "SUSHI", 18),
            "CRV" => new Currency("CRV", "Curve", "CRV", 18),
            "YFI" => new Currency("YFI", "yearn.finance", "YFI", 18),
            "SNX" => new Currency("SNX", "Synthetix", "SNX", 18),
            "UMA" => new Currency("UMA", "UMA", "UMA", 18),
            "BAL" => new Currency("BAL", "Balancer", "BAL", 18),
            "REN" => new Currency("REN", "Ren", "REN", 18),
            "KNC" => new Currency("KNC", "Kyber Network", "KNC", 18),
            "ZRX" => new Currency("ZRX", "0x", "ZRX", 18),
            "BAND" => new Currency("BAND", "Band Protocol", "BAND", 18),
            "LRC" => new Currency("LRC", "Loopring", "LRC", 18),
            "STORJ" => new Currency("STORJ", "Storj", "STORJ", 8),
            "BAT" => new Currency("BAT", "Basic Attention Token", "BAT", 18),
            "ENJ" => new Currency("ENJ", "Enjin Coin", "ENJ", 18),
            "MANA" => new Currency("MANA", "Decentraland", "MANA", 18),
            "SAND" => new Currency("SAND", "The Sandbox", "SAND", 18),
            "AXS" => new Currency("AXS", "Axie Infinity", "AXS", 18),
            "CHZ" => new Currency("CHZ", "Chiliz", "CHZ", 18),
            "DOGE" => new Currency("DOGE", "Dogecoin", "Ð", 8),
            "SHIB" => new Currency("SHIB", "Shiba Inu", "SHIB", 18),
            "USDT" => new Currency("USDT", "Tether", "₮", 6),
            "USDC" => new Currency("USDC", "USD Coin", "USDC", 6),
            "BUSD" => new Currency("BUSD", "Binance USD", "BUSD", 18),
            "DAI" => new Currency("DAI", "Dai", "DAI", 18),
            "FRAX" => new Currency("FRAX", "Frax", "FRAX", 18),
            "LUSD" => new Currency("LUSD", "Liquity USD", "LUSD", 18),
            "MIM" => new Currency("MIM", "Magic Internet Money", "MIM", 18),
            "FEI" => new Currency("FEI", "Fei USD", "FEI", 18),
            "TRIBE" => new Currency("TRIBE", "Tribe", "TRIBE", 18),
            "OHM" => new Currency("OHM", "Olympus", "OHM", 9),
            "TIME" => new Currency("TIME", "Wonderland", "TIME", 9),
            "KLIMA" => new Currency("KLIMA", "KlimaDAO", "KLIMA", 9),
            "SPELL" => new Currency("SPELL", "Spell Token", "SPELL", 18),
            "ICE" => new Currency("ICE", "IceToken", "ICE", 18),
            _ => throw new ArgumentException($"Devise non supportée: {code}", nameof(code))
        };
    }

    /// <summary>
    /// Devises les plus communes
    /// </summary>
    public static Currency EUR => FromCode("EUR");
    public static Currency USD => FromCode("USD");
    public static Currency GBP => FromCode("GBP");
    public static Currency CHF => FromCode("CHF");
    public static Currency JPY => FromCode("JPY");

    /// <summary>
    /// Validation de la devise
    /// </summary>
    private void ValidateCurrency()
    {
        if (string.IsNullOrWhiteSpace(Code) || Code.Length != 3)
            throw new ArgumentException("Le code devise doit faire exactement 3 caractères", nameof(Code));

        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Le nom de la devise ne peut pas être vide", nameof(Name));

        if (string.IsNullOrWhiteSpace(Symbol))
            throw new ArgumentException("Le symbole de la devise ne peut pas être vide", nameof(Symbol));

        if (DecimalPlaces < 0 || DecimalPlaces > 18)
            throw new ArgumentException("Le nombre de décimales doit être entre 0 et 18", nameof(DecimalPlaces));
    }

    /// <summary>
    /// Égalité des devises
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Currency currency && Code == currency.Code;
    }

    /// <summary>
    /// Hash code
    /// </summary>
    public override int GetHashCode()
    {
        return Code.GetHashCode();
    }

    /// <summary>
    /// Représentation textuelle
    /// </summary>
    public override string ToString()
    {
        return $"{Code} ({Symbol})";
    }

    /// <summary>
    /// Opérateurs d'égalité
    /// </summary>
    public static bool operator ==(Currency? left, Currency? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Code == right.Code;
    }

    public static bool operator !=(Currency? left, Currency? right)
    {
        return !(left == right);
    }
}