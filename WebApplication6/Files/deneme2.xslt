<?xml version="1.0" encoding="UTF-8"?>
<!-- ?2018 Foriba Consulting -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/Ogrenciler">
    <html>
      <head>
        <style> body {background-color:red;}</style>
      </head>
      <body>
        <table>
          <tr>
            <td>Ad</td>
            <td>Soyad</td>
            <td>Numara</td>
          </tr>
          <xsl:for-each select="Ogrenci">
            <tr>
              <td>
                <xsl:value-of select="ad" />
              </td>
              <td>
                <xsl:value-of select="soyad" />
              </td>
              <td>
                <xsl:value-of select="numara" />
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>