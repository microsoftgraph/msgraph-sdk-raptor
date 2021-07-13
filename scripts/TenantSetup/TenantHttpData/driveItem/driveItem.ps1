# Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
Param(
    [string] $IdentifiersPath = (Join-Path $PSScriptRoot "../../../../msgraph-sdk-raptor-compiler-lib/identifiers.json" -Resolve)
)
$raptorUtils = Join-Path $PSScriptRoot "../../RaptorUtils.ps1" -Resolve
. $raptorUtils

$identifiers = Get-CurrentIdentifiers -IdentifiersPath $IdentifiersPath

if($null -eq $driveItem) {
$driveItem = Request-DelegatedResource -Uri "me/drive/root/children?`$filter=name eq 'Contoso Purchasing Data - Q1.xlsx'" |
    Select-Object -First 1
}
$driveItem.id
$identifiers.driveItem._value = $driveItem.id

if($null -eq $driveItemPermission) {
$driveItemPermission = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/permissions" |
    Select-Object -First 1
}
$driveItemPermission.id
$identifiers.driveItem.permission._value = $driveItemPermission.id

if($null -eq $driveItemThumbnail) {
$driveItemThumbnail = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/thumbnails?`$top=1" |
    Select-Object -First 1
}
$driveItemThumbnail.id
$identifiers.driveItem.thumbnailSet._value = $driveItemThumbnail.id
$identifiers.driveItem.thumbnailSet.thumbnailSet._value = "small"

if($null -eq $driveItemVersion) {
$driveItemVersion = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/versions" |
    Select-Object -First 1
}
$driveItemVersion.id
$identifiers.driveItem.driveItemVersion._value = $driveItemVersion.id

if($null -eq $driveItemWorkbookTable) {
$driveItemWorkbookTable = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/tables" |
    Where-Object { $_.name -eq "Table1" } |
    Select-Object -First 1
}
$driveItemWorkbookTable.id
$identifiers.driveItem.workbookTable._value = $driveItemWorkbookTable.id

if($null -eq $tableColumn) {
$tableColumn = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/tables/$($driveItemWorkbookTable.id)/columns" |
    Select-Object -First 1
}
$tableColumn.id
$identifiers.driveItem.workbookTable.workbookTableColumn._value = $tableColumn.id

if($null -eq $tableRow) {
$tableRow = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/tables/$($driveItemWorkbookTable.id)/rows" |
    Select-Object -First 1
}
$tableRow.index
$identifiers.driveItem.workbookTable.workbookTableRow._value = $tableRow.index

if($null -eq $worksheet) {
$worksheet = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/worksheets" |
    Where-Object { $_.name -eq "Sheet1" } |
    Select-Object -First 1
}
$worksheet.id
$identifiers.driveItem.workbookWorksheet._value = $worksheet.id

if (!$workbookNamedItem){
    $namedItemData = Get-RequestData -ChildEntity "workbookNamedItem"
    try{
        $workbookNamedItem = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/names/add" -Method "POST" -Body $namedItemData
    }
    catch{
        $workbookNamedItem = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/names/$($namedItemData.name)"
    }
}

$workbookNamedItem.name
$identifiers.driveItem.workbookNamedItem._value = $workbookNamedItem.name

if (!$namedItemFormatBorder){
    $namedItemFormatBorder = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/names/$($workbookNamedItem.name)/range/format/borders?`$top=1" |
        Select-Object -First 1
}
$namedItemFormatBorder.id
$identifiers.driveItem.workbookNamedItem.workbookRangeBorder._value = $namedItemFormatBorder.id

if (!$workbookChart){
    $chartBody = Get-RequestData -ChildEntity "workbookChart"
    $workbookChart = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/worksheets/$($worksheet.id)/charts/add" -Method "POST" -Body $chartBody
}
$workbookChart.name
$identifiers.driveItem.workbookWorksheet.workbookChart._value = $workbookChart.name

if (!$workbookChartSeries){
    $workbookChartSeries = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/worksheets/$($worksheet.id)/charts/$($workbookChart.name)/series"|
        Select-Object -First 1
}
$workbookChartSeries.name
$identifiers.driveItem.workbookWorksheet.workbookChart.workbookChartSeries._value = $workbookChartSeries.name

if(!$workbookChartPoint){
    $workbookChartPoint = Request-DelegatedResource -Uri "me/drive/items/$($driveItem.id)/workbook/worksheets/$($worksheet.id)/charts/$($workbookChart.name)/series/$($workbookChartSeries.name)/points"|
        Select-Object -First 1
}
$workbookChartPoint.id
$identifiers.driveItem.workbookWorksheet.workbookChart.workbookChartSeries.workbookChartPoint._value = $workbookChartPoint.id

# if (!$driveItemWorkbookOperation){
#     $driveItemWorkbookOperation = # No data, reliant on TODO Printer object
# }
# $driveItemWorkbookOperation.id
# $identifiers.driveItem.workbookOperation._value = $driveItemWorkbookOperation.id

# if (!$workbookComment){
#     $workbookComment = #No Data, Does create workbook comment api exist??
# }
# $workbookComment.id
# $identifiers.driveItem.workbookComment._value = $workbookComment.id

# if (!$workbookCommentReply){
#     $workbookCommentReply = #No Data, reliant on existence of workbook comment
# }
# $workbookCommentReply.id
# $identifiers.driveItem.workbookComment.workbookCommentReply._value = $workbookCommentReply.id





$identifiers | ConvertTo-Json -Depth 10 > $identifiersPath
