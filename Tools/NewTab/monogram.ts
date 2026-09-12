// Raster port of gfx::DrawMonogramInCanvas (Windows uses semibold Segoe UI).
export function drawMonogram(data){
  const size=Math.ceil(24*(window.devicePixelRatio||1));
  const canvas=document.createElement('canvas');canvas.width=size;canvas.height=size;
  const ctx=canvas.getContext('2d');
  ctx.fillStyle=data.color;
  ctx.beginPath();ctx.roundRect(0,0,size,size,Math.floor(size/2));ctx.fill();
  ctx.fillStyle='#fff';ctx.font=`600 ${Math.trunc(size*.5)}px "Segoe UI"`;
  ctx.textAlign='center';ctx.textBaseline='alphabetic';
  const metrics=ctx.measureText(data.text);
  const ascent=metrics.fontBoundingBoxAscent,descent=metrics.fontBoundingBoxDescent;
  ctx.fillText(data.text,size/2,(size-ascent-descent)/2+ascent);
  return canvas.toDataURL('image/png');
}
